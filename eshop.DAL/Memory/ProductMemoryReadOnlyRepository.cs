using eshop.Core;

namespace eshop.DAL.Memory
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в памяти
    /// </summary>
    internal class ProductMemoryReadOnlyRepository : IRepository<Product>
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
        public IReadOnlyCollection<Product> GetAll()
        {
            return _products.AsReadOnly();
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            return _products.Count;
        }

        /// <inheritdoc/>
        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(item => item.Id == id);
        }

        public void Update(Product item)
        {
            throw new NotSupportedException("Запись товаров в памяти не поддерживается");
        }

        public int Insert(Product item)
        {
            throw new NotSupportedException("Запись товаров в памяти не поддерживается");
        }

        public Task UpdateAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertAsync(Product item)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetAll());
        }

        public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetCount());
        }

        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetById(id));
        }
    }
}
