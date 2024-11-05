**План занятия:**

- Добавить обработку исключений
- Добавить FluentResult

1. Добавляем обработку исключений при выполнении команд в `ConsolePage`:

    ```csharp
        public async Task WaitForInput(CancellationToken ct)
        {
            while (true)
            {    
                (IEshopCommand command, string[]? args) nextCommand = default;
                while (nextCommand == default)
                {
                    Console.WriteLine("Выполните команду");
                    var command = Console.ReadLine();
                    nextCommand = GetNextCommand(command, _initialCommand as ICommandWithContext);
                }

                if (nextCommand.command is GoToRootPageCommand && _prev is not null)
                {
                    _lastCommandIsGoToRoot = true;
                    _prev._lastCommandIsGoToRoot = true;
                    break;
                }

                if (nextCommand.command is BackCommand && _prev is not null)
                    break;

                try
                {
                    if (nextCommand.command is ICommandWithCommandsList commandWithContext)
                    {
                        if (nextCommand.command.GetType() != _initialCommand?.GetType())

                        {
                            await commandWithContext.ExecuteAsync(nextCommand.args, CancellationToken.None);
                            if (!commandWithContext.ExecutionSuccess)
                            {
                                DisplayInitial();
                                Console.WriteLine($"Ошибка: {commandWithContext.Result}");
                                continue;
                            }
                            var nextPage = new ConsolePage(_context, commandWithContext, nextCommand.args, this);
                            nextPage.DisplayInitial();
                            await nextPage.WaitForInput(ct);
                            DisplayInitial();
                            if (_prev is null || nextPage._lastCommandIsGoToRoot)
                                break;

                            continue;
                        }

                        _initialCommand = commandWithContext;
                        _args = nextCommand.args;
                        await _initialCommand.ExecuteAsync(_args, CancellationToken.None);
                        DisplayInitial();




                    }
                    else
                    {
                        DisplayInitial();

                        if (nextCommand.command != null)
                        {
                            await nextCommand.command.ExecuteAsync(nextCommand.args, CancellationToken.None);
                            Console.WriteLine(nextCommand.command.Result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Произошила ошибка при выполнении команды: {ex.Message}");
                    Console.WriteLine("Нажмите Enter для продолжения...");
                    Console.ReadLine();
                }
            }
        }
    ```

2. Добавляем глобальную обработку исключений в `Program`:

    ```csharp
        public static async Task Main(string[] args)
        {
            try
            {
                var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

                var app = new ApplicationContext(confBuilder);

                Console.WriteLine(ApplicationContext.Title);
                var initialCommand = app.GetInitialCommand();
                await initialCommand.ExecuteAsync(null, CancellationToken.None);
                var page = new ConsolePage(app, initialCommand, null);

                while (true)
                {
                    page.DisplayInitial();
                    await page.WaitForInput(CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"""
                    Произошила ошибка в работе приложения: {ex.Message}
                    { ex.StackTrace}
                    """);
                Console.WriteLine("Нажмите Enter чтобы закрыть приложение ...");
                Console.ReadLine();
            }
        }
    ```

3. Создаем новый проект `eshop.Application`, который будет содержаться хэндлеры команд и запросов. Новый проект зависит от проекта `eshop.DAL`

4. Подключаем нугет пакет с `FluentResults` в новом проекте

5. Реализуем хэндлер для получения списка торговых единиц в новом проекте:

    ```csharp
        namespace eshop.Application.SaleItems
        {
            public class GetSaleItemHandler
            {
                private readonly RepositoryFactory _repositoryFactory;

                public GetSaleItemHandler(RepositoryFactory repositoryFactory)
                {
                    _repositoryFactory = repositoryFactory;
                }

                public async Task<Result<IEnumerable<SaleItemDto>>> GetItemsAsync(ItemTypes itemType, int? count)
                {
                    var repository = _repositoryFactory.CreateSaleItemRepository();

                    try
                    {
                        var items = (await repository
                            .GetAllAsync())
                            .Where(i => i.ItemType == itemType);

                        var requestedItems = count is null or <= 0
                            ? items
                            : items.Take(count.Value);

                        return Result.Ok(requestedItems
                            .Select(i => new SaleItemDto(i.ItemType, i.Id, i.Name, i.Price, (i as Product)?.Stock)));
                    }
                    catch (Exception ex)
                    {
                        return Result.Fail("Не удалось получить коллекцию торговых единиц")
                            .WithError(ex.Message)
                            .WithError(ex.StackTrace);
                    }
                }
            }
        }
    ```

    ```csharp
        namespace eshop.Application.SaleItems
        {
            public record SaleItemDto(ItemTypes ItemType, int Id, string Name, decimal Price, decimal? Stock = null);
        }
    ```

6. Добавляем хэндлер в DI-контейнер:

    ```csharp
        public ApplicationContext(IConfiguration configuration)
        {
            var services = new ServiceCollection()
                .AddScoped<RepositoryFactory>((sp) =>
                {
                    return new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? "");
                })
                .AddScoped<GetSaleItemHandler>();

            _serviceProvider = services.BuildServiceProvider();
        }
    ```

7. Подключаем проект `eshop.Application` как зависимость в `eshop`

8. Изменяем команду `DisplayProductsCommand` на использование нового хэндлера:

    ```csharp
        public DisplayProductsCommand(GetSaleItemHandler getSaleItemHandler)
        {
            _getSaleItemHandler = getSaleItemHandler;
        }

        ...

        public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
        {
            _ = int.TryParse(args?.FirstOrDefault(), out var count);

            var items = await _getSaleItemHandler.GetItemsAsync(ItemTypes.Product, count);

            if (items.IsFailed)
            {
                Result = "Не удалось получить список товаров";
                return;
            }

            var message = new StringBuilder("Товары:").AppendLine();
            
            for (var i = 0; i < items.Value.Count(); i++)
            {
                var item = items.Value.ElementAt(i);
                message
                    .Append($"{item.Id}. {item.Name}. Цена: {item.Price}. Остатки: {item.Stock}")
                    .AppendLine();
            }

            Result = message.ToString();
        }
    ```

9. Изменяем команду `DisplayServicesCommand` аналогичным образом:

    ```csharp
        public DisplayServicesCommand(GetSaleItemHandler getSaleItemHandler)
        {
            _getSaleItemHandler = getSaleItemHandler;
        }

        ...

        public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
        {
            _ = int.TryParse(args?.FirstOrDefault(), out var count);

            var items = await _getSaleItemHandler.GetItemsAsync(ItemTypes.Service, count);

            if (items.IsFailed)
            {
                Result = "Не удалось получить список услуг";
                return;
            }

            var message = new StringBuilder("Услуги:").AppendLine();

            for (var i = 0; i < items.Value.Count(); i++)
            {
                var item = items.Value.ElementAt(i);
                message
                    .Append($"{item.Id}. {item.Name}. Цена: {item.Price}")
                    .AppendLine();
            }

            Result = message.ToString();
        }
    ```

10. Удаляем метод `GetDisplayText` из модели `SaleItem` и всех наследников.

    Доменная модель больше не отвечает за представление разных видов торговых единиц. За это отвечает только слой представления (консоль, API и т.д.)