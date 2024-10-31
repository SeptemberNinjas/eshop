using eshop.Core;

namespace eshop.DAL
{
    /// <summary>
    /// Абстрактная фабрика для создания репозиториев
    /// </summary>    
    public abstract class RepositoryFactory
    {
        /// <summary>
        /// Создать репозиторий для работы с товарными единицами
        /// </summary>
        public abstract IReadOnlyRepository<SaleItem> CreateSaleItemRepository();

        public abstract IRepositoryAsync<Product> CreateProductAsyncRepositoy();

        /// <summary>
        /// Создать репозиторий для работы с остатками
        /// </summary>
        public abstract IRepository<Stock> CreateStockRepository();

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
