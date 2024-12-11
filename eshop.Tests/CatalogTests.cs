using eshop.Application.SaleItems;
using eshop.Core;
using eshop.DAL.Database;
using eshop.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace eshop.Tests
{
    [TestFixture]
    public class CatalogTests
    {
        private IServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public void Init()
        {
            _serviceProvider = new ServiceCollection()
                .AddSingleton<IReadOnlyRepository<SaleItem>, SaleItemRepositoryMock>()
                .AddSingleton<ILogger<GetSaleItemHandler>, NullLogger<GetSaleItemHandler>>()
                .AddScoped<DatabaseContext>(_ => Mock.Of<DatabaseContext>())
                .AddScoped<GetSaleItemHandler>()
                .BuildServiceProvider();
        }

        [Test(Description = "Получение списка товаров")]
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
    }
}
