**План занятия:**

- Реализовать хранение товаров и услуг с использованием паттерна "Репозиторий"
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

            /// <summary>
            /// Добавить новый элемент
            /// </summary>
            /// <param name="item"></param>
            void Insert(T item);
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
        internal class ProductInMemoryRepository : IRepository<Product>
        {
            private readonly List<Product> _products;

            public ProductInMemoryRepository()
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

7. Пишем реализацию для хранения услуг - `ServiceInMemoryRepository` - аналогичным образом

8. Пишем абстрактный класс `RepositoryFactory` для реализации паттерна "Фабричный метод" и создания нужных репозиториев:

    ```csharp
        /// <summary>
        /// Абстрактная фабрика для создания репозиториев
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class RepositoyFactory<T>
        {
            public abstract IRepository<T> Create();
        }
    ```

9. Пишем реализацию фабрики для создания репозиториев хранения товаров и услуг:

    Для товаров:

    ```csharp
        /// <summary>
        /// Реализация фабрики для хранения товаров в памяти
        /// </summary>
        public class ProductInMemoryRepositoryFactory : RepositoyFactory<Product>
        {
            public override IRepository<Product> Create()
            {
                return new ProductInMemoryRepository();
            }
        }
    ```

    Для услуг:

    ```csharp
        /// <summary>
        /// Реализация фабрики для хранения услуг в памяти
        /// </summary>
        public class ServiceInMemoryRepositoryFactory : RepositoyFactory<Service>
        {
            public override IRepository<Service> Create()
            {
                return new ServiceInMemoryRepository();
            }
        }
    ```

10. Добавляем создание репозиториев в `ApplicationContext`:

    Добавляем поля для хранения фабрик создания репозиториев:

    ```csharp
        /// <summary>
        /// Фабрика, для создания репозитория с продуктами
        /// </summary>
        private readonly RepositoyFactory<Product> _productRepositoyFactory;

        /// <summary>
        /// Фабрика, для создания репозитория с услугами
        /// </summary>
        private readonly RepositoyFactory<Service> _serviceRepositoryFactory;
    ```

    Добавляем конструктор по умолчанию:

    ```csharp
        public ApplicationContext()
        {
            _productRepositoyFactory = new ProductInMemoryRepositoryFactory();
            _serviceRepositoryFactory = new ServiceInMemoryRepositoryFactory();

            _products = _productRepositoyFactory.Create();
            _services = _serviceRepositoryFactory.Create();
        }
    ```

11. Пишем реализацию хранения товаров в Json-файлах:

    ```csharp
        /// <summary>
        /// Реализация репозитория для хранения товаров в json'е
        /// </summary>
        internal class ProductInJsonRepository : IRepository<Product>
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

            private IEnumerable<Product> GetProducts()
            {
                if (!File.Exists("products.json"))
                {
                    using var sw = new StreamWriter("products.json");
                    sw.WriteLine("[]");
                }
                
                using var sr = new StreamReader("products.json");

                var result = JsonSerializer.Deserialize<IEnumerable<Product>>(sr.BaseStream);

                return (IReadOnlyCollection<Product>)(result ?? []);
            }
        }
    ```

12. Пишем фабрику для создания нового репозитория

    ```csharp
        /// <summary>
        /// Реализация фабрики для хранения товаров в json'е
        /// </summary>
        public class ProductInJsonRepositoryFactory : RepositoyFactory<Product>
        {
            public override IRepository<Product> Create()
            {
                return new ProductInJsonRepository();
            }
        }
    ```

13. Меняем реализацию фабрики в `ApplicationContext`:

    ```csharp
        _productRepositoyFactory = new ProductInJsonRepositoryFactory();
    ```

14. Запускаем приложением и пытаемся просмотреть список товаров.

    Если мы все сделали правильно, то у нас должен быть пустой список товаров.

15. Наполняем файл `products.json`, который лежит в папке в `Debug` данные о товарах:

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
