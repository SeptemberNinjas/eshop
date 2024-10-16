namespace eshop.Core.DAL.InMemory
{
    /// <summary>
    /// Реализация фабрики для хранения товаров в памяти
    /// </summary>
    public class ProductInMemoryRepositoryFactory : RepositoyFactory<Product>
    {
        public override IRepository<Product> Create()
        {
            return new ProductInMemoryRepository();
        }
    }
}
