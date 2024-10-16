namespace eshop.Core.DAL.Json
{
    /// <summary>
    /// Реализация фабрики для хранения товаров в json'е
    /// </summary>
    public class ProductInJsonRepositoryFactory : RepositoyFactory<Product>
    {
        public override IRepository<Product> Create()
        {
            return new ProductInJsonRepository();
        }
    }
}
