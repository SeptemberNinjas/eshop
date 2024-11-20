using eshop.Application.Order;
using eshop.Application.Payment;
using eshop.Core;
using eshop.DAL;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.Tests
{
    [TestFixture]
    public class PaymentTests
    {
        private IServiceProvider _serviceProvider;

        private readonly List<Order> _orders = [];

        [OneTimeSetUp]
        public void Init()
        {
            var orderRepository = new Mock<IRepository<Order>>();

            orderRepository
                .Setup(item => item.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns((int orderId, CancellationToken cancellationToken) =>
                {
                    return Task.FromResult(_orders.FirstOrDefault());
                });

            orderRepository
                .Setup(item => item.UpdateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var repositoryFactory = new Mock<RepositoryFactory>();

            repositoryFactory
                .Setup(item => item.CreateOrdersRepository())
                .Returns(() => orderRepository.Object);

            _serviceProvider = new ServiceCollection()
                .AddScoped(sp => repositoryFactory.Object)
                .AddScoped<PayOrderByCashHandler>()
                .AddScoped<PayOrderByCashlessHandler>()
                .BuildServiceProvider();
        }

        [SetUp]
        public void Setup()
        {
            var order = new Order(1, OrderStatus.New, CatalogHelper.Catalog.Take(1).Select(item => new ItemsListLine(item, 1)));

            _orders.Add(order);
        }

        [TearDown]
        public void Teardown()
        {
            _orders.Clear();
        }

        [Test(Description = "Оплата наличными")]
        public async Task PayByCashSuccess()
        {
            var amount = GetAmountOfAllOrders();

            var scope = _serviceProvider.CreateScope();

            var payOrder = scope.ServiceProvider.GetRequiredService<PayOrderByCashHandler>();

            var payOrderResult = await payOrder.PayHandler(1, amount, CancellationToken.None);

            Assert.That(payOrderResult.IsSuccess, Is.True, payOrderResult.ToString());
        }

        [Test(Description = "Оплата безналичным способом")]
        public async Task PayByCashlessSuccess()
        {
            var amount = GetAmountOfAllOrders();

            var scope = _serviceProvider.CreateScope();

            var payOrder = scope.ServiceProvider.GetRequiredService<PayOrderByCashlessHandler>();

            var payOrderResult = await payOrder.PayHandler(1, amount, CancellationToken.None);

            Assert.That(payOrderResult.IsSuccess, Is.True, payOrderResult.ToString());
        }

        private decimal GetAmountOfAllOrders() => _orders
                .SelectMany(order => order.Lines)
                .Sum(item => item.Count * item.SaleItem.Price);
    }
}
