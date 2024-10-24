using eshop.Core;

namespace eshop.DAL.Json
{
    /// <summary>
    /// Реализация фабрики для хранения в json'е
    /// </summary>
    public class JsonRepositoryFactory : RepositoryFactory
    {
        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository() => new ProductJsonReadOnlyRepository();

        public override IRepositoryAsync<Product> CreateProductAsyncRepositoy()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IReadOnlyRepository<Service> CreateServiceRepository() => new ServiceJsonReadOnlyRepository();

        /// <inheritdoc/>
        public override IRepository<Basket> CreateBasketRepository() => new BasketJsonRepository();

        /// <inheritdoc />
        public override IRepository<Order> CreateOrdersRepository() => new OrdersJsonRepository();
    }
}
