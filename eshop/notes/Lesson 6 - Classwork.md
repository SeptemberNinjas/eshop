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

8. 