using eshop.Core;

namespace eshop.DAL.Memory
{
    /// <summary>
    /// Реализация фабрики для хранения в памяти
    /// </summary>
    public class MemoryRepositoryFactory : RepositoryFactory
    {
        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository()
        {
            return new ProductMemoryReadOnlyRepository();
        }

        /// <inheritdoc/>
        public override IReadOnlyRepository<Service> CreateServiceRepository()
        {
            return new ServiceMemoryReadOnlyRepository();
        }

        /// <inheritdoc/>
        public override IRepository<Basket> CreateBasketRepository()
        {
            throw new NotSupportedException("Работа с корзиной в памяти не поддерживается");
        }

        /// <inheritdoc />
        public override IRepository<Order> CreateOrdersRepository()
        {
            throw new NotSupportedException("Работа с заказами в памяти не поддерживается");
        }
    }
}
