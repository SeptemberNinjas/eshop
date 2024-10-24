using eshop.Core;

namespace eshop.DAL
{
    /// <summary>
    /// Абстрактная фабрика для создания репозиториев
    /// </summary>    
    public abstract class RepositoryFactory
    {
        /// <summary>
        /// Создать репозиторий для работы с товарами
        /// </summary>
        public abstract IRepository<Product> CreateProductRepository();

        public abstract IRepositoryAsync<Product> CreateProductAsyncRepositoy();

        /// <summary>
        /// Создать репозиторий для работы с услугами
        /// </summary>
        public abstract IReadOnlyRepository<Service> CreateServiceRepository();

        /// <summary>
        /// Создать репозиторий для работы с корзиной
        /// </summary>
        public abstract IRepository<Basket> CreateBasketRepository();

        /// <summary>
        /// Создать репозиторий для работы с заказами
        /// </summary>
        public abstract IRepository<Order> CreateOrdersRepository();
    }
}
