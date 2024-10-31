using eshop.Core;

namespace eshop.DAL.Json
{
    /// <summary>
    /// Реализация фабрики для хранения в json'е
    /// </summary>
    public class JsonRepositoryFactory : RepositoryFactory
    {
        /// <inheritdoc/>
        public override IReadOnlyRepository<SaleItem> CreateSaleItemRepository() => new ProductJsonReadOnlyRepository();

        /// <inheritdoc/>
        public override IRepository<Stock> CreateStockRepository() => throw new NotSupportedException("Не реализуем устаревшие репозитории, т.к. в ближайшем ДЗ они будут удалены");

        /// <inheritdoc/>
        public override IRepository<Basket> CreateBasketRepository() => new BasketJsonRepository();

        /// <inheritdoc />
        public override IRepository<Order> CreateOrdersRepository() => new OrdersJsonRepository();
    }
}
