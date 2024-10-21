using eshop.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        public override IRepository<Order> CreateOrdersRepository()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository()
        {
            return new ProductDatabaseRepository(_connectionString);
        }

        /// <inheritdoc/>
        public override IRepository<Service> CreateServiceRepository()
        {
            return new ServiceDatabaseRepository(_connectionString);
        }
    }
}
