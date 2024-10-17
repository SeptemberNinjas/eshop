namespace eshop.Core.DAL.Memory
{
    /// <summary>
    /// Реализация фабрики для хранения в памяти
    /// </summary>
    public class MemoryRepositoryFactory : RepositoyFactory
    {
        /// <inheritdoc/>
        public override IRepository<Product> CreateProductRepository()
        {
            return new ProductMemoryRepository();
        }

        /// <inheritdoc/>
        public override IRepository<Service> CreateServiceRepository()
        {
            return new ServiceMemoryRepository();
        }
    }
}
