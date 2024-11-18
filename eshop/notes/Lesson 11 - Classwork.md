**План занятия:**

- Реализовать несколько тестов на базе NUnit со следующими типами заглушек:

  - самописные заглушки
  - библиотеку Moq

1. Создаем новый проект `eshop.Tests` в существующем солюшене.

    При создании проекта используем шаблон `Тестовый проект NUnit`

2. Реализуем статичный класс-хелпер для хранения каталога:

    ```csharp
        public static class CatalogHelper
        {
            public static IEnumerable<SaleItem> Catalog => [
                    new Product(1, "Валик", 19.99M, 3),
                    new Product(2, "Краскопуль", 200M, 4),
                    new Service(3, "Побелить стену", 54.99M),
                    new Service(4, "Покрастить дом", 500M)
                ];
        }
    ```

3. Реализуем заглушку для интерфейса `IReadOnlyRepository<SaleItem>`:

    ```csharp
        public class SaleItemRepositoryMock : IReadOnlyRepository<SaleItem>
        {
            public Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult((IReadOnlyCollection<SaleItem>)CatalogHelper.Catalog);
            }

            public Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }
        }
    ```

4. Реализуем заглушку для абстрактного класса `RepositoryFactory`:

    ```csharp
        public class RepositoryFactoryMock : RepositoryFactory
        {
            public override IRepository<Basket> CreateBasketRepository()
            {
                throw new NotImplementedException();
            }

            public override IRepository<Order> CreateOrdersRepository()
            {
                throw new NotImplementedException();
            }

            public override IReadOnlyRepository<SaleItem> CreateSaleItemRepository()
            {
                return new SaleItemRepositoryMock();
            }

            public override IRepository<Stock> CreateStockRepository()
            {
                throw new NotImplementedException();
            }
        }
    ```

5. Устанавливаем nuget-пакет `Microsoft.Extensions.DependencyInjection` в проект `eshop.Tests`

    Пакет необходим для использования DI-контейнера в тестах.

6. Создаем класс `CatalogTests`, в котором будут находиться тесты для каталога.

    ```csharp
        [TestFixture]
        public class CatalogTests
        {
        }
    ```

7. Реалиуем метод первоначальной настройки тестов каталога.

    Метод будет подготавливать DI-контейнер с необходимыми реализациями зависимостей.

    ```csharp
        [TestFixture]
        public class CatalogTests
        {
            private IServiceProvider _serviceProvider;

            [OneTimeSetUp]
            public void Init()
            {
                _serviceProvider = new ServiceCollection()
                    .AddScoped<RepositoryFactory, RepositoryFactoryMock>()
                    .AddScoped<GetSaleItemHandler>()
                    .BuildServiceProvider();
            }
        }
    ```

8. Реализуем тест получения коллекции товаров:

    ```csharp
        [Test(Description = "Получеие списка товаров")]
        public async Task GetAllProductsSuccess()
        {
            using var scope = _serviceProvider.CreateScope();

            var getSaleItemHandler = scope.ServiceProvider.GetRequiredService<GetSaleItemHandler>();

            var result = await getSaleItemHandler.GetItemsAsync(ItemTypes.Product, null, CancellationToken.None);

            var productCount = CatalogHelper.Catalog.Count(item => item.ItemType == ItemTypes.Product);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True, "Вернулся неуспешный результат");
                Assert.That(productCount, Is.EqualTo(result.Value.Count()), "Вернулось некорректное количество товаров");
            });
        }
    ```

9. Устанавливаем nuget-пакет `Moq`

10. Создаем класс `BasketTests`, в котором будут находиться тесты для корзины:

    ```csharp
        [TestFixture]
        public class BasketTests
        {
        }
    ```

11. Реализуем метод первоначальной настройки тестов корзины:

    ```csharp
        [TestFixture]
        public class BasketTests
        {
            private IServiceProvider _serviceProvider;

            private Basket _basket;

            [OneTimeSetUp]
            public void Init()
            {
                var basketRepository = new Mock<IRepository<Basket>>();

                _basket = new Basket(1, []);

                basketRepository
                    .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
                    .Returns((CancellationToken cancellationToken) =>
                    {
                        return Task.FromResult((IReadOnlyCollection<Basket>)[_basket]);
                    });

                var repositoryFactory = new Mock<RepositoryFactory>();

                repositoryFactory
                    .Setup(item => item.CreateBasketRepository())
                    .Returns(() => basketRepository.Object);

                repositoryFactory
                    .Setup(item => item.CreateSaleItemRepository())
                    .Returns(() => new SaleItemRepositoryMock());

                _serviceProvider = new ServiceCollection()
                    .AddScoped(sp => repositoryFactory.Object)
                    .AddScoped<GetBasketHandler>()
                    .AddScoped<AddBasketLineHandler>()
                    .BuildServiceProvider();
            }
        }
    ```

12. Реализуем два тесты для корзины:

    Получение пустой корзины:

    ```csharp
        [Test(Description = "Получение пустой корзины")]
        [Order(1)]
        public async Task GetEmptyBasketSuccess()
        {
            var scope = _serviceProvider.CreateScope();

            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();

            var result = await getBasketHandler.GetBasketAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailed, Is.True, "Не удалось получить ошибку при получении несуществующей корзины");
            });
        }
    ```

    Добавление товара в корзину:

    ```csharp
        [Test(Description = "Добавление товара в корзину")]
        [Order(2)]
        [TestCase(2, 2)]
        public async Task AddLineToBasketSuccess(int saleItemId, int count)
        {
            var scope = _serviceProvider.CreateScope();

            var addBasketLineHandler = scope.ServiceProvider.GetRequiredService<AddBasketLineHandler>();
            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();

            var addLineResult = await addBasketLineHandler.AddLineAsync(saleItemId, count, CancellationToken.None);
            var getBasketResult = await getBasketHandler.GetBasketAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(addLineResult.IsSuccess, Is.True, "Не удалось добавить товар в корзину");
                Assert.That(getBasketResult.IsSuccess, Is.True, "Не удалось получить коризну");
                Assert.That(getBasketResult.Value.Lines.Count, Is.EqualTo(1), "В корзине некорректное количество товаров");
            });
        }
    ```

13. В моке для `IReadOnlyRepository<SaleItem>` реализуем метод получения товара по id:

    ```csharp
        public Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CatalogHelper.Catalog.FirstOrDefault(item => item.Id == id));
        }
    ```