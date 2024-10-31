using eshop.Core;

namespace eshop.DAL.Memory
{
    /// <summary>
    /// Реализация фабрики для хранения в памяти
    /// </summary>
    public class MemoryRepositoryFactory : RepositoryFactory
    {
        /// <inheritdoc/>
        public override IReadOnlyRepository<SaleItem> CreateSaleItemRepository()
        {
            return new ProductMemoryReadOnlyRepository();
        }

        public override IReadOnlyRepositoryAsync<SaleItem> CreateSaleItemAsyncRepository()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override IRepository<Stock> CreateStockRepository()
        {
            throw new NotSupportedException("Не реализуем устаревшие репозитории, т.к. в ближайшем ДЗ они будут удалены");
        }

        /// <inheritdoc/>
        public override IRepository<Basket> CreateBasketRepository()
        {
            throw new NotSupportedException("Работа с корзиной в памяти не поддерживается");
        }

        /// <inheritdoc />
        public override IRepository<Order> CreateOrdersRepository()
        {
            throw new NotSupportedException("Работа с заказами в памяти не поддерживается");
        }
    }
}
