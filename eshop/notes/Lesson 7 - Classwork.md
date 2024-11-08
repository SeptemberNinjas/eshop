**План занятия:**

- Перевести работу с товарами и услугами на асинхронные вызовы
- Реализовать Func делегат для обработки результатов обращения к БД
- Написать расширение для получения текстового значения для ItemType

1. Описываем асинхронные методы в `IRepository<T>` и `IReadOnlyRepository<T>` для реализации асинхронных методов работы:

    ```csharp
        public interface IReadOnlyRepository<T>
        {
            ...

            /// <summary>
            /// Вернуть список всех элементов
            /// </summary>
            Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);

            /// <summary>
            /// Получить количество элементов
            /// </summary>
            Task<int> GetCountAsync(CancellationToken cancellationToken = default);

            /// <summary>
            /// Найти элемент по идентификатору
            /// </summary>
            /// <param name="id">Идентификатор</param>
            Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        }
    ```

    ```csharp
        public interface IRepositoryAsync<T> : IReadOnlyRepository<T>
        {
            ...

            /// <summary>
            /// Обновить объект
            /// </summary>
            Task UpdateAsync(T item);

            /// <summary>
            /// Добавить новый объект
            /// </summary>
            /// <returns>Идентификатор созданного объекта</returns>
            Task<int> InsertAsync(T item);
        }
    ```

2. Описываем в `DatabaseContext` Func-делегата для выполнения команд к БД:

    ```csharp
        ...

        private async Task<NpgsqlConnection> GetConnectionAsync()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);

            await _connection.OpenAsync();

            return _connection;
        } 

        /// <summary>
        /// Получить команду для СУБД
        /// </summary>
        protected NpgsqlCommand GetCommand(string text)
        {
            return new NpgsqlCommand
            {
                Connection = GetConnection(),
                CommandType = CommandType.Text,
                CommandText = text
            };
        }

        protected async Task<List<T>> ExecuteReaderListAsync<T>(string commandText, Func<DbDataReader, T> binging, CancellationToken cancellationToken)
        {
            using var connection = await GetConnectionAsync();

            var command = GetCommand(commandText);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var result = new List<T>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(binging(reader));
            }

            return result;
        }

        protected async Task<T?> ExecuteReaderAsync<T>(string commandText, Func<DbDataReader, T> binding, CancellationToken cancellationToken)
        {
            using var connection = await GetConnectionAsync();

            var command = GetCommand(commandText);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return binding(reader);


            return default;
        }
    ```

3. Реализуем новые интерфейсы в `SaleItemDataRepository`:

    ```csharp
        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var commandText =
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id";

            var result = await ExecuteReaderListAsync(commandText, GetSaleItem, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            var commandText = 
                "select count(*) from catalog";

            var result = await ExecuteReaderAsync(commandText, (reader) =>
            {
                return int.TryParse(reader[0]?.ToString(), out var count) ? count : 0;
            }, cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var commandText =
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}";

            var result = await ExecuteReaderAsync(commandText, GetSaleItem, cancellationToken);

            return result;
        }
    ```

4. Делаем заглушки в остальных реализация репозиториев (Database, Json, Memory):

    ```csharp
        public Task UpdateAsync(Stock item)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Stock item)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Stock>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Stock?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    ```

5. Описываем асинхронный метод в интерфейсе `IEshopCommand`:

    ```csharp
        Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
        {
            Execute(args);
            return Task.CompletedTask;
        }
    ```

6. Описываем реализацию нового метода в командах `DisplayProductsCommand` и `DisplayServicesCommand`:

    Для `DisplayProductsCommand`:

    ```csharp
        public bool ExecutionSuccess { get; private set; } = true;

        public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
        {
            var allItems = (await _saleItems.GetAllAsync(cancellationToken))
                .Where(i => i.ItemType is ItemTypes.Product)
                .ToArray();
            if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
            {
                count = allItems.Length;
            }

            var message = new StringBuilder("Товары:").AppendLine();
            for (var i = 0; i < Math.Min(allItems.Length, count); i++)
            {
                message.AppendLine(allItems[i].GetDisplayText());
            }

            Result = message.ToString();
        }

        public void Execute(string[]? args)
        {
            ExecuteAsync(args, CancellationToken.None).Wait();
        }
    ```

    Для `DisplayServicesCommand`:

    ```csharp
        public async Task ExecuteAsync(string[]? args, CancellationToken cancellationToken)
        {
            var allItems = (await _saleItems.GetAllAsync(cancellationToken))
                .Where(i => i.ItemType is ItemTypes.Service)
                .ToArray();

            if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
            {
                count = allItems.Length;
            }

            var message = new StringBuilder("Услуги:").AppendLine();
            for (var i = 0; i < Math.Min(allItems.Length, count); i++)
            {
                message.AppendLine(allItems[i].GetDisplayText());
            }

            Result = message.ToString();
        }

        public void Execute(string[]? args)
        {
            ExecuteAsync(args, CancellationToken.None).Wait();
        }
    ```

7. Переводим `ConsolePage` и `Program` на работу с асинхронными методами:

    Для `ConsolePage`:

    ```csharp
        public async Task WaitForInput(CancellationToken ct)
        {
            ...

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
                nextCommand.command?.Execute(nextCommand.args);
                Console.WriteLine(nextCommand.command?.Result);
            }

            ...
        }
    ```

    Для `Program`:

    ```csharp
        public static async Task Main(string[] args)
        {
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var app = new ApplicationContext(confBuilder);

            Console.WriteLine(ApplicationContext.Title);
            var initialCommand = app.GetInitialCommand();
            initialCommand.Execute(null);
            var page = new ConsolePage(app, initialCommand, null);
            
            while (true)
            {
                page.DisplayInitial();
                await page.WaitForInput(CancellationToken.None);
            }
        }
    ```

8. Добавляем метод расширения для отображения текстового названия продажной единицы:

    ```csharp
        public static class ItemTypesExtensions
        {
            public static string GetDisplayText(this ItemTypes itemType)
            {
                return itemType switch
                {
                    ItemTypes.Product => "Товар",
                    ItemTypes.Service => "Услуга",
                    _ => ""
                };
            }
        }
    ```

9. Используем расширение в `ItemsListLine`:

    ```csharp
        public string Text => $"{ItemType.GetDisplayText()}: {SaleItem?.Name} | Цена: {SaleItem?.Price:F2} | Кол-во: {Count}";
    ```
