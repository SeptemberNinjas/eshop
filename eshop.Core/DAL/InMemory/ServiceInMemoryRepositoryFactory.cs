namespace eshop.Core.DAL.InMemory
{
    /// <summary>
    /// Реализация фабрики для хранения услуг в памяти
    /// </summary>
    public class ServiceInMemoryRepositoryFactory : RepositoyFactory<Service>
    {
        public override IRepository<Service> Create()
        {
            return new ServiceInMemoryRepository();
        }
    }
}
