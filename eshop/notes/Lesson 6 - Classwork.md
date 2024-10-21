**План занятия:**

- sfdsf

1. Устанавливаем wsl и докер-декстоп

2. Пишем `docker-compose.yml`:

    ```yml
        version: '3.9'

        services:
          pg_eshop:
            image: postgres:17.0
            environment:
              POSTGRES_DB: "eshop"
              POSTGRES_PASSWORD: "password"
            ports:
              - "5432:5432"
            volumes:
              - pgdata:/data/postgres
              - ./init-scripts:/docker-entrypoint-initdb.d
            
        volumes:
          pgdata:
    ```

3. Рядом с файлом `docker-compose.yml` создаем каталог `init-scripts` и размещаем в нем файл `init.sql` со следующим содержимым:

    ```sql
        create table "catalog"(
            "id" int not null,
            "name" varchar(500) not null,
            "price" money not null,
            "type" smallint not null
        );

        create table "stock"(
            "id" int not null,
            "amount" int not null
        );

        insert into "catalog"("id", "name", "price", "type")
        values 
            (1, 'Лопата', 9.99, 1),
            (2, 'Трактор', 300, 1),
            (3, 'Раскопать яму', 5.49, 2),
            (4, 'Вспахать поле', 1000, 2);

        insert into "stock"("id", "amount")
        values
            (1, 3),
            (2, 4);
    ```

4. Поднимаем контейнер с Postgres'ом:

    ```bash
        docker-compose up -d
    ```

5. Подключаем с СУБД и убеждаемся что:

    - БД `eshop` создана
    - структура таблиц создана
    - в таблицах есть данные

6. Настраиваем хранение конфигурации в проекте:

    - Устанавливаем пакеты:
        - Microsoft.Extensions.Configuration
        - Microsoft.Extensions.Configuration.Json

    - Реализуем хранение конфигурации в `ApplicationContext`:
        - добавляем новое приватное поле

            ```csharp
                /// <summary>
                /// Конфигурация приложения
                /// </summary>
                private readonly IConfiguration _configuration;
            ```

        - Меняем конструктор

            ```csharp
                public ApplicationContext(IConfiguration configuration)
                {
                    _configuration = configuration;

                    _repositoryFactory = new JsonRepositoryFactory();

                    _products = _repositoryFactory.CreateProductRepository();
                    _services = _repositoryFactory.CreateServiceRepository();
                }
            ```

    - Реализуем чтение конфигурации из файла `appsettings.json` в `eshop.Program.Main`:

        ```csharp
            var confBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var app = new ApplicationContext(confBuilder);
        ```

7. Подключаем пакет `Npgsql` в прокет `eshop.DAL`

8. Пишем контекст подключения к СУБД `DatabaseContext`:

    ```csharp
        /// <summary>
        /// Контекст подключения к СУБД
        /// </summary>
        internal class DatabaseContext : IDisposable
        {
            private readonly string _connectionString;

            private NpgsqlConnection? _connection;

            public DatabaseContext(string connectionString)
            {
                _connectionString = connectionString;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
                _connection?.Dispose();
            }

            /// <summary>
            /// Получить соединение с БД
            /// </summary>
            /// <returns></returns>
            public NpgsqlConnection GetConnection()
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                    return _connection;

                _connection = new NpgsqlConnection(_connectionString);

                _connection.Open();

                return _connection;
            }

            /// <summary>
            /// Получить команду для СУБД
            /// </summary>
            /// <param name="connection"></param>
            /// <param name="text"></param>
            /// <returns></returns>
            public NpgsqlCommand GetCommand(string text)
            {
                return new NpgsqlCommand
                {
                    Connection = GetConnection(),
                    CommandType = CommandType.Text,
                    CommandText = text
                };
            }
        }
    ```

9. Пишем реализацию репозитория для работы со списком товаров:

    ```csharp
        /// <summary>
        /// Реализация репозитория для хранения товаров в БД
        /// </summary>
        internal class ProductDatabaseRepository : DatabaseContext, IRepository<Product>
        {
            public ProductDatabaseRepository(string connectionString) : base(connectionString) { }
            
            /// <inheritdoc/>
            public IReadOnlyCollection<Product> GetAll()
            {
                using var command = GetCommand(
                    @"select c.*, s.amount 
                        from catalog c
                            left join stock s on c.Id = s.Id
                        where type = 1");

                using var reader = command.ExecuteReader();

                var result = new List<Product>();

                while (reader.Read())
                {
                    result.Add(GetProduct(reader));
                }

                return result;
            }

            /// <inheritdoc/>
            public Product? GetById(int id)
            {
                using var command = GetCommand(
                    $@"select c.*, s.amount 
                        from catalog c
                            left join stock s on c.Id = s.Id
                        where type = 1 and c.id = {id}");

                using var reader = command.ExecuteReader();

                if (reader.Read())
                    return GetProduct(reader);

                return null;
            }

            /// <inheritdoc/>
            public int GetCount()
            {
                using var command = GetCommand(
                    "select count(*) from catalog where type = 1");

                var result = command.ExecuteScalar();

                if (int.TryParse(result?.ToString(), out int count))
                    return count;
                else
                    return 0;
            }

            public int Insert(Product item)
            {
                throw new NotImplementedException();
            }

            public void Update(Product item)
            {
                throw new NotImplementedException();
            }

            private static Product GetProduct(NpgsqlDataReader reader)
            {
                return new Product(
                        reader.GetFieldValue<int>("id"),
                        reader.GetFieldValue<string>("name"),
                        reader.GetFieldValue<decimal>("price"),
                        reader.GetFieldValue<int>("amount"));
            }
        }
    ```

10. Пишем реализацию работы с услугами аналогичным образом

11. Пишем фабрику для создания новых репозиториев:

    ```csharp
        public class DatabaseRepositoryFactory : RepositoryFactory
        {
            private readonly string _connectionString;

            public DatabaseRepositoryFactory(string connectionString)
            {
                _connectionString = connectionString;
            }

            public override IRepository<Basket> CreateBasketRepository()
            {
                throw new NotImplementedException();
            }

            public override IRepository<Order> CreateOrdersRepository()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public override IRepository<Product> CreateProductRepository()
            {
                return new ProductDatabaseRepository(_connectionString);
            }

            /// <inheritdoc/>
            public override IRepository<Service> CreateServiceRepository()
            {
                return new ServiceDatabaseRepository(_connectionString);
            }
        }
    ```

12. Меняем реализацию фабрики в `ApplicationContext`:

    ```csharp
        _repositoryFactory = new DatabaseRepositoryFactory(configuration["ConnectionString"] ?? "");
    ```
