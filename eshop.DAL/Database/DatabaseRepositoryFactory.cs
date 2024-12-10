using eshop.Core;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация фабрики для хранение в базе данных
    /// </summary>
    public class DatabaseRepositoryFactory : RepositoryFactory
    {
        private DatabaseContext _databaseContext;

        public DatabaseRepositoryFactory(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public override IRepository<Basket> CreateBasketRepository()
        {
            return new BasketDatabaseRepository(_databaseContext);
        }

        public override IRepository<Order> CreateOrdersRepository()
        {
            return new OrderDatabaseRepository(_databaseContext);
        }

        /// <inheritdoc/>
        public override IReadOnlyRepository<SaleItem> CreateSaleItemRepository()
        {
            return new SaleItemDatabaseRepository(_databaseContext);
        }

        /// <inheritdoc/>
        public override IRepository<Stock> CreateStockRepository()
        {
            return new StockDatabaseRepository(_databaseContext);
        }
    }
}
