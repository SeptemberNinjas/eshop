using eshop.Application.Order;
using eshop.Core;
using eshop.DAL;
using eshop.DAL.Database;
using eshop.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eshop.Tests
{
    [TestFixture]
    public class OrderTests
    {
        private IServiceProvider _serviceProvider;

        private readonly List<Basket> _baskets = [];

        private readonly List<Order> _orders = [];

        [OneTimeSetUp]
        public void Init()
        {
            var basketRepository = new Mock<IRepository<Basket>>();

            basketRepository
                .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken cancellationToken) =>
                {
                    return Task.FromResult((IReadOnlyCollection<Basket>)_baskets);
                });

            basketRepository
                .Setup(item => item.UpdateAsync(It.IsAny<Basket>(), It.IsAny<CancellationToken>()))
                .Returns((Basket basket, CancellationToken cancellationToken) =>
                {
                    var oldBasket = _baskets.FirstOrDefault(item => item.Id == basket.Id);
                    oldBasket = basket;

                    return Task.CompletedTask;
                });

            var stockRepository = new Mock<IRepository<Stock>>();

            stockRepository
                .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken cancellationToken) =>
                {
                    return Task.FromResult((IReadOnlyCollection<Stock>)CatalogHelper.Stocks.ToList());
                });

            stockRepository
                .Setup(item => item.UpdateAsync(It.IsAny<Stock>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var orderRepository = new Mock<IRepository<Order>>();

            orderRepository
                .Setup(item => item.InsertAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns((Order order, CancellationToken cancellationToken) =>
                {
                    _orders.Add(order);
                    return Task.FromResult(0);
                });

            orderRepository
                .Setup(item => item.GetAllAsync(It.IsAny<CancellationToken>()))
                .Returns((CancellationToken cancellationToken) =>
                {
                    return Task.FromResult((IReadOnlyCollection<Order>)_orders);
                });

            _serviceProvider = new ServiceCollection()
                .AddSingleton<ILogger<GetBasketHandler>, NullLogger<GetBasketHandler>>()
                .AddSingleton<ILogger<CreateOrderHandler>, NullLogger<CreateOrderHandler>>()
                .AddSingleton<ILogger<GetOrdersHandler>, NullLogger<GetOrdersHandler>>()
                .AddScoped<DatabaseContext>(_ => new DatabaseContext(null))
                .AddScoped<IRepository<Basket>>(_ => basketRepository.Object)
                .AddScoped<IRepository<Order>>(_ => orderRepository.Object)
                .AddScoped<IRepository<Stock>>(_ => stockRepository.Object)
                .AddScoped<GetBasketHandler>()
                .AddScoped<CreateOrderHandler>()
                .AddScoped<GetOrdersHandler>()
                .BuildServiceProvider();
        }

        private void PrepareBasket(int productCount)
        {
            var product = CatalogHelper.Catalog.FirstOrDefault(item => item.Id == 1) as Product;
            var service = CatalogHelper.Catalog.FirstOrDefault(item => item.Id == 3) as Service;

            var basket = new Basket(1, [
                new ItemsListLine(product!, productCount),
                new ItemsListLine(service!)
            ], "customer", DateTime.UtcNow);

            _baskets.Add(basket);
        }

        [TearDown]
        public void TearDown()
        {
            _baskets.Clear();
            _orders.Clear();
        }

        [Test(Description = "Создать заказ и получить список заказов")]
        public async Task CreateOrderSuccess()
        {
            PrepareBasket(3);

            var scope = _serviceProvider.CreateScope();

            var createOrderHandler = scope.ServiceProvider.GetRequiredService<CreateOrderHandler>();
            var getBasketHandler = scope.ServiceProvider.GetRequiredService<GetBasketHandler>();
            var getOrdersHandler = scope.ServiceProvider.GetRequiredService<GetOrdersHandler>();

            var createOrderResult = await createOrderHandler.CreateOrderAsync("customer", CancellationToken.None);
            var getBasketResult = await getBasketHandler.GetBasketAsync("customer", CancellationToken.None);
            var getOrdersResult = await getOrdersHandler.GetOrdersAsync(CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(createOrderResult.IsSuccess, Is.True, createOrderResult.ToString());
                Assert.That(getBasketResult.Value.Items, Is.Empty, "Корзина не пустая");
                Assert.That(getOrdersResult.IsSuccess, Is.True, getOrdersResult.ToString());
                Assert.That(getOrdersResult.Value.Count(), Is.EqualTo(1), "Некорректное количество заказов");
            });
        }

        [Test(Description = "Попытка создать заказ на недостаточное количество товара")]
        public async Task CreateOrderForInsufficientProductCount()
        {
            PrepareBasket(4);

            var scope = _serviceProvider.CreateScope();

            var createOrderHandler = scope.ServiceProvider.GetRequiredService<CreateOrderHandler>();

            var result = await createOrderHandler.CreateOrderAsync("customer", CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsFailed, Is.True, "Заказ был создан");
                Assert.That(result.HasError(err => "Недостаточно товара".Equals(err.Message)), Is.True,
                    "Отсутствует ошибка о недостаточности товара");
            });
        }
    }
}