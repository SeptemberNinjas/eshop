namespace eshop.Core.DAL.Memory
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в памяти
    /// </summary>
    internal class ProductMemoryRepository : IRepository<Product>
    {
        private readonly List<Product> _products;

        public ProductMemoryRepository()
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
    }
}
