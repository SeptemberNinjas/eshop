**План занятия:**

- Перевести работу с БД на асинхронные вызовы
- Реализовать Func делегат для обработки результатов обращения к БД
- Добавить обработку исключений при обращении к БД
- Добавить глобальную обработку исключений для всего приложения
- Написать расширение для получения текстового значения для ItemType
- Реализовать паттерн Result как альтернативу обработки исключений для управления ходом приложения

1. Описываем новые интерфейсы `IRepositoryAsync<T>` и `IReadOnlyRepositoryAsync<T>` для реализации асинхронных методов работы:

    ```csharp
        public interface IReadOnlyRepositoryAsync<T>
        {
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
        public interface IRepositoryAsync<T> : IReadOnlyRepositoryAsync<T>
        {
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

2. Реализуем новые интерфейсы в `ProductDatabaseRepository`:

    ```csharp
        #region Async

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1");

            using var reader = await command.ExecuteReaderAsync();

            var result = new List<Product>();

            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(GetProduct(reader));
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}");

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return GetProduct(reader);

            return null;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            using var command = GetCommand(
                "select count(*) from catalog where type = 1");

            var result = await command.ExecuteScalarAsync(cancellationToken);

            return int.TryParse(result?.ToString(), out var count) ? count : 0;
        }

        public Task UpdateAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Product item)
        {
            throw new NotImplementedException();
        }

        #endregion
    ```

3. Правим реализацию синхронных интерфейсов, для устранения дублирования кода:

    ```csharp
        public IReadOnlyCollection<Product> GetAll()
        {
            return Task.Run(async () =>
            {
                return await GetAllAsync();
            }).Result;
        }

        public int GetCount()
        {
            return Task.Run(async () =>
            {
                return await GetCountAsync();
            }).Result;
        }

        public Product? GetById(int id)
        {
            return Task.Run(async () =>
            {
                return await GetByIdAsync(id);
            }).Result;
        }

        IReadOnlyCollection<SaleItem> IReadOnlyRepository<SaleItem>.GetAll()
        {
            return GetAll();
        }

        SaleItem? IReadOnlyRepository<SaleItem>.GetById(int id)
        {
            return GetById(id);
        }
    ```

4. Добавляем метод сохрания асинхронного репозитория в фабрике `RepositoryFactory`:

    ```csharp
        public abstract IRepositoryAsync<Product> CreateProductAsyncRepositoy();
    ```

5. Описываем реализацию нового метода:

    Для `Json` и `Memory` репозиториев без поддержки асинхронных методов:

    ```csharp
        public override IRepositoryAsync<Product> CreateProductAsyncRepositoy()
        {
            throw new NotImplementedException();
        }
    ```

    Для `Database` репозиториев:

    ```csharp
        public override IRepositoryAsync<Product> CreateProductAsyncRepositoy()
        {
            return new ProductDatabaseRepository(_connectionString);
        }
    ```

6. Переводим команду `DisplayProductsCommand` на работу с асинхронным репозиторием:

    ```csharp
        private readonly IReadOnlyRepositoryAsync<Product> _products;

        /// <inheritdoc cref="DisplayProductsCommand"/>
        public DisplayProductsCommand(IReadOnlyRepositoryAsync<Product> products)
        {
            _products = products;
        }

        ...

        public void Execute(string[]? args)
        {
            Task.Run(async () =>
            {
                if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
                {
                    count = await _products.GetCountAsync();
                }

                var allItems = _products.GetAllAsync().Result;

                var message = new StringBuilder("Товары:").AppendLine();
                for (var i = 0; i < Math.Min(await _products.GetCountAsync(), count); i++)
                {
                    message.AppendLine(allItems.ElementAt(i).GetDisplayText());
                }

                Result = message.ToString();
            }).Wait();
        }
    ```

7. Добавляем в `ApplicationContext` создание асинхронного репозитория:

    ```csharp
        CommandType.DisplayProducts => new DisplayProductsCommand(repositoryFactory.CreateProductAsyncRepositoy()),
    ```

8. Пишем обобщенные методы для выполнения запросов к БД:

    ```csharp
        private async Task<NpgsqlConnection> GetConnectionAsync()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
                return _connection;

            _connection = new NpgsqlConnection(_connectionString);

            await _connection.OpenAsync();

            return _connection;
        } 

        protected async Task<List<T>> ExecuteReaderListAsync<T>(string commandText, CancellationToken cancellationToken, Func<DbDataReader, T> binging)
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

        protected async Task<T?> ExecuteReaderAsync<T>(string commandText, CancellationToken cancellationToken, Func<DbDataReader, T> binding)
        {
            using var connection = await GetConnectionAsync();

            var command = GetCommand(commandText);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
                return binding(reader);


            return default;
        }
    ```

9. Переписываем репозиторий для товаров на использование обобщенных методов для выполнения запросов к БД:

    ```csharp
        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var commandText = 
                @"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1";

            var result = await ExecuteReaderListAsync(commandText, cancellationToken, GetProduct);

            return result;
        }

        /// <inheritdoc/>
        public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var commandText =
                $@"select c.*, s.amount 
                    from catalog c
                        left join stock s on c.Id = s.Id
                    where type = 1 and c.id = {id}";

            var result = await ExecuteReaderAsync(commandText, cancellationToken, GetProduct);

            return result;
        }

        /// <inheritdoc/>
        public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            var commandText=
                "select count(*) from catalog where type = 1";

            var result = await ExecuteReaderAsync(commandText, cancellationToken, (reader) =>
            {
                return int.TryParse(reader[0]?.ToString(), out var count) ? count : 0;
            });

            return result;
        }
    ```

10. Добавляем в команду `DisplayProductsCommand` обработчик исключений при выполнении команды:

    ```csharp
        try
        {
            Task.Run(async () =>
            {
                if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
                {
                    count = await _products.GetCountAsync();
                }

                var allItems = _products.GetAllAsync().Result;

                var message = new StringBuilder("Товары:").AppendLine();
                for (var i = 0; i < Math.Min(await _products.GetCountAsync(), count); i++)
                {
                    message.AppendLine(allItems.ElementAt(i).GetDisplayText());
                }

                Result = message.ToString();
            }).Wait();
        }
        catch (Exception ex)
        {
            ExecutionSuccess = false;
            Result = ex.Message;
        }
    ```

11. Добавляем глобальную обработку ошибок в `Program`:

    ```csharp
        while (true)
        {
            try
            {
                page.DisplayInitial();
                page.WaitForInput();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошила ошибка при выполнении команды: {ex.Message}");
                Console.WriteLine("Нажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }
    ```

12. Добавляем метод расширения для отображения текстового названия продажной единицы:

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

13. Используем расширение в `ItemsListLine`:

    ```csharp
        public string Text => $"{ItemType.GetDisplayText()}: {SaleItem?.Name} | Цена: {SaleItem?.Price:F2} | Кол-во: {Count}";
    ```

14. В некотором роде паттерн Result у нас реализован в виде свойства `ExecutionSuccess` в командах с интерфейсом `ICommandWithCommandsList`
