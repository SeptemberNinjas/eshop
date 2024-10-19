**План занятия:**

- Реализовать работу со списком товаров и услуг через обобщенный интерфейс.
- Написать две реализации хранения:

  - хранение товаро и услуг в памяти
  - хранение товаро и услуг в json-файле

Пишем код:

1. Создаем новый обощенный интерфейс `IRepository<T>`

    ```csharp
        /// <summary>
        /// Обобщенный интерфейс репозитория для взаимодействия с сущностями интернет-магазина
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public interface IRepository<T>
        {
            /// <summary>
            /// Вернуть список всех элементов
            /// </summary>
            /// <returns></returns>
            IReadOnlyCollection<T> GetAll();

            /// <summary>
            /// Получить количество элементов
            /// </summary>
            /// <returns></returns>
            int GetCount();

            /// <summary>
            /// Найти элемент по идентификатору
            /// </summary>
            /// <param name="id">Идентификатор</param>
            /// <returns></returns>
            T? GetById(int id);
        }
    ```

2. Меняем в `ApplicationContext` хранение товаров и услуг с массивов на новый интерфейс:

    ```csharp
        /// <summary>
        /// Список товаров
        /// </summary>
        private readonly IRepository<Product> _products;

        /// <summary>
        /// Список услуг
        /// </summary>
        private readonly IRepository<Service>_services;
    ```

3. Меняем передачу параметров в вызовах команды `AddBasketLineCommand`:

    ```csharp
        CommandType.AddProductToBasket => new AddBasketLineCommand(_basket, _products.GetAll().ToArray()),
        CommandType.AddServiceToBasket => new AddBasketLineCommand(_basket, _services.GetAll().ToArray()),
    ```

4. Изменяем команду `DisplayProductsCommand`:

    - Меняем массив товаров на интерфейс
    - Меняем аргументы конструктора

    ```csharp
        private readonly IRepository<Product> _products;

        /// <inheritdoc cref="DisplayProductsCommand"/>
        public DisplayProductsCommand(IRepository<Product> products)
        {
            _products = products;
        }
    ```

    - Дорабатываем метод `Execute`

    ```csharp
        public void Execute(string[]? args)
        {
            if (args is null || args.Length == 0 || !int.TryParse(args[0], out var count) || count < 1)
            {
                count = _products.GetCount();
            }

            var allItems = _products.GetAll();

            var message = new StringBuilder("Товары:").AppendLine();
            for (var i = 0; i < Math.Min(_products.GetCount(), count); i++)
            {
                message.AppendLine(allItems.ElementAt(i).GetDisplayText());
            }

            Result = message.ToString();
        }
    ```

5. Изменяем команду `DisplayServicesCommand` аналогичным образом

6. Пишем реализацию репозитория для хранения товаров в памяти приложения:

    ```csharp
        /// <summary>
        /// Реализация репозитория для хранения товаров в памяти
        /// </summary>
        internal class ProductMemoryRepository : IRepository<Product>
        {
            private readonly List<Product> _products;

            public ProductMemoryRepository()
            {
                _products =
                [
                    new Product(1, "Лопата", 9.99m, 3),
                    new Product(2, "Трактор", 300, 4)
                ];
            }

            /// <inheritdoc/>
            public IReadOnlyCollection<Product> GetAll()
            {
                return _products.AsReadOnly();
            }

            /// <inheritdoc/>
            public int GetCount()
            {
                return _products.Count;
            }

            /// <inheritdoc/>
            public Product? GetById(int id)
            {
                return _products.FirstOrDefault(item => item.Id == id);
            }

            /// <inheritdoc/>
            public void Insert(Product item)
            {
                _products.Add(item);
            }
        }
    ```

7. Пишем реализацию для хранения услуг - `ServiceMemoryRepository` - аналогичным образом

8. Пишем абстрактный класс `RepositoryFactory` для реализации паттерна "Фабричный метод" и создания нужных репозиториев:

    ```csharp
        /// <summary>
        /// Абстрактная фабрика для создания репозиториев
        /// </summary>    
        public abstract class RepositoyFactory
        {
            /// <summary>
            /// Создать репозиторий для работы с товарами
            /// </summary>
            /// <returns></returns>
            public abstract IRepository<Product> CreateProductRepository();

            /// <summary>
            /// Создать репозиторий для работы с услугами
            /// </summary>
            /// <returns></returns>
            public abstract IRepository<Service> CreateServiceRepository();
        }
    ```

9. Пишем реализацию фабрики для создания репозиториев хранения товаров и услуг:

    Для товаров:

    ```csharp
        /// <summary>
        /// Реализация фабрики для хранения в памяти
        /// </summary>
        public class MemoryRepositoryFactory : RepositoyFactory
        {
            /// <inheritdoc/>
            public override IRepository<Product> CreateProductRepository()
            {
                return new ProductMemoryRepository();
            }

            /// <inheritdoc/>
            public override IRepository<Service> CreateServiceRepository()
            {
                return new ServiceMemoryRepository();
            }
        }
    ```

10. Добавляем создание репозиториев в `ApplicationContext`:

    Добавляем поле для хранения фабрик создания репозиториев:

    ```csharp
        /// <summary>
        /// Фабрика, для создания репозиторией
        /// </summary>
        private readonly RepositoyFactory _repositoryFactory;
    ```

    Добавляем конструктор по умолчанию:

    ```csharp
        public ApplicationContext()
        {
            _repositoryFactory = new MemoryRepositoryFactory();

            _products = _repositoryFactory.CreateProductRepository();
            _services = _repositoryFactory.CreateServiceRepository();
        }
    ```

11. Пишем реализацию хранения товаров в Json-файлах:

    ```csharp
        /// <summary>
        /// Реализация репозитория для хранения товаров в json'е
        /// </summary>
        internal class ProductJsonRepository : IRepository<Product>
        {
            /// <inheritdoc/>
            public IReadOnlyCollection<Product> GetAll()
            {
                return (IReadOnlyCollection<Product>)GetProducts();
            }

            /// <inheritdoc/>
            public Product? GetById(int id)
            {
                var products = GetProducts();

                return products.FirstOrDefault(item => item.Id == id);
            }

            /// <inheritdoc/>
            public int GetCount()
            {
                var products = GetProducts();

                return products.Count();
            }

            /// <inheritdoc/>
            public void Insert(Product item)
            {
                throw new NotImplementedException();
            }

            private static IEnumerable<Product> GetProducts()
            {
                if (!File.Exists("data\\products.json"))
                {
                    using var sw = new StreamWriter("data\\products.json");
                    sw.WriteLine("[]");
                }
                
                using var sr = new StreamReader("data\\products.json");

                var result = JsonSerializer.Deserialize<IEnumerable<Product>>(sr.BaseStream);

                return (IReadOnlyCollection<Product>)(result ?? []);
            }
        }
    ```

12. Пишем реализацию хранения услуг в Json-файлах аналогичным образом

13. Создаем json-файлы `products.json` и `services.json` в самом проект в папке `data` и наполняем его данными.

    Контент для товаров:

    ```json
        [
            {
                "Id": 1,
                "Name": "Лопата",
                "Price": 9.99,
                "Stock": 3
            },
            {
                "Id": 2,
                "Name": "Трактор",
                "Price": 300,
                "Stock": 4
            }
        ]
    ```

    Контент для услуг:

    ```json
        [
            {
                "Id": 3,
                "Name": "Раскопать яму",
                "Price": 5.49
            },
            {
                "Id": 4,
                "Name": "Вспахать поле",
                "Price": 1000
            }
        ]
    ```

    Для файла устанавливаем свойство - `Всегда копировать`

14. Пишем фабрику для создания нового репозитория

    ```csharp
        /// <summary>
        /// Реализация фабрики для хранения в json'е
        /// </summary>
        public class JsonRepositoryFactory : RepositoyFactory
        {
            /// <inheritdoc/>
            public override IRepository<Product> CreateProductRepository()
            {
                return new ProductJsonRepository();
            }

            /// <inheritdoc/>
            public override IRepository<Service> CreateServiceRepository()
            {
                return new ServiceJsonRepository();
            }
        }
    ```

15. Меняем реализацию фабрики в `ApplicationContext`:

    ```csharp
        _repositoryFactory = new JsonRepositoryFactory();
    ```

16. Запускаем приложением и пытаемся просмотреть список товаров и услуг.
