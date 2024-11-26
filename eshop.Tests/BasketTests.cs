using eshop.Application.Order;
using eshop.Core;
using eshop.DAL;
using eshop.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace eshop.Tests
{
    [TestFixture]
    public class BasketTests
    {
        private IServiceProvider _serviceProvider;

        private Basket _basket;

        [OneTimeSetUp]
        public void Init()
        {
            var basketRepository = new Mock<IRepository<Basket>>();

            _basket = new Basket(1, [], "customer");

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
                .AddScoped<ClearBasketHandler>()
                .BuildServiceProvider();
        }

        [Test(Description = "Получение пустой корзины")]
        [Order(1)]
        public async Task GetEmptyBasketSuccess()
        {
            var scope = _serviceProvider.CreateScope();

            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();

            var result = await getBasketHandler.GetBasketAsync("customer", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True, "Не удалось получить корзину");
                Assert.That(result.Value.Items, Is.Empty, "Линии в пустой корзине");
            });
        }

        [Test(Description = "Добавление товара в корзину")]
        [Order(2)]
        [TestCase(2, 2)]
        public async Task AddLineToBasketSuccess(int saleItemId, int count)
        {
            var scope = _serviceProvider.CreateScope();

            var addBasketLineHandler = scope.ServiceProvider.GetRequiredService<AddBasketLineHandler>();
            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();

            var addLineResult = await addBasketLineHandler.AddLineAsync("customer", saleItemId, count, CancellationToken.None);
            var getBasketResult = await getBasketHandler.GetBasketAsync("customer", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(addLineResult.IsSuccess, Is.True, addLineResult.ToString());
                Assert.That(getBasketResult.IsSuccess, Is.True, getBasketResult.ToString());
                Assert.That(getBasketResult.Value.Items.Count, Is.EqualTo(1), "В корзине некорректное количество товаров");
            });
        }

        [Test(Description = "Очистка корзины")]
        [Order(3)]
        public async Task ClearBasketSuccess()
        {
            var scope = _serviceProvider.CreateScope();

            var clearBasketHandler = scope.ServiceProvider.GetRequiredService<ClearBasketHandler>();
            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();

            var clearBasketResult = await clearBasketHandler.ClearBasketAsync("customer", CancellationToken.None);
            var getBasketResult = await getBasketHandler.GetBasketAsync("customer", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(clearBasketResult.IsSuccess, Is.True, "Не удалось очистить корзину");
                Assert.That(getBasketResult.IsSuccess, Is.True, "Не удалось получить корзину");
                Assert.That(getBasketResult.Value.Items, Is.Empty, "Линии в пустой корзине");
            });
        }
    }
}
