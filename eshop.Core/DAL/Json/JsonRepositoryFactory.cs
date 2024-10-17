namespace eshop.Core.DAL.Json
{
    /// <summary>
    /// Реализация фабрики для хранения в json'е
    /// </summary>
    public class JsonRepositoryFactory : RepositoyFactory
    {
        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository()
        {
            return new ProductJsonRepository();
        }

        /// <inheritdoc/>
        public override IRepository<Service> CreateServiceRepository()
        {
            return new ServiceJsonRepository();
        }
    }
}
