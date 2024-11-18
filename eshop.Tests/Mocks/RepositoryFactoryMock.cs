using eshop.Core;
using eshop.DAL;

namespace eshop.Tests.Mocks
{
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
}
