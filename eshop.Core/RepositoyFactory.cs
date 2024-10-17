namespace eshop.Core
{
    /// <summary>
    /// Абстрактная фабрика для создания репозиториев
    /// </summary>    
    public abstract class RepositoyFactory
    {
        /// <summary>
        /// Создать репозиторий для работы с товарами
        /// </summary>
        /// <returns></returns>
        public abstract IRepository<Product> CreateProductRepository();

        /// <summary>
        /// Создать репозиторий для работы с услугами
        /// </summary>
        /// <returns></returns>
        public abstract IRepository<Service> CreateServiceRepository();
    }
}
