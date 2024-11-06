using eshop.Core;

namespace eshop.DAL.Memory
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в памяти
    /// </summary>
    internal class ProductMemoryReadOnlyRepository : IReadOnlyRepository<SaleItem>
    {
        private readonly List<Product> _products;

        public ProductMemoryReadOnlyRepository()
        {
            _products =
            [
                new Product(1, "Лопата", 9.99m, 3),
                new Product(2, "Трактор", 300, 4)
            ];
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<SaleItem> GetAll()
        {
            return _products.AsReadOnly();
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            return _products.Count;
        }

        /// <inheritdoc/>
        public SaleItem? GetById(int id)
        {
            return _products.FirstOrDefault(item => item.Id == id);
        }

        public void Update(SaleItem item)
        {
            throw new NotSupportedException("Запись товаров в памяти не поддерживается");
        }

        public int Insert(SaleItem item)
        {
            throw new NotSupportedException("Запись товаров в памяти не поддерживается");
        }

        public Task<IReadOnlyCollection<SaleItem>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SaleItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
