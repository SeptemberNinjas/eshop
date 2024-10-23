using eshop.Core;

namespace eshop.DAL.Database
{
    /// <summary>
    /// Реализация фабрики для хранение в базе данных
    /// </summary>
    public class DatabaseRepositoryFactory : RepositoryFactory
    {
        private readonly string _connectionString;

        public DatabaseRepositoryFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override IRepository<Basket> CreateBasketRepository()
        {
            return new BasketDatabaseRepository(_connectionString);
        }

        public override IRepository<Order> CreateOrdersRepository()
        {
            return new OrderDatabaseRepository(_connectionString);
        }

        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository()
        {
            return new ProductDatabaseRepository(_connectionString);
        }

        /// <inheritdoc/>
        public override IReadOnlyRepository<Service> CreateServiceRepository()
        {
            return new ServiceDatabaseRepository(_connectionString);
        }
    }
}
