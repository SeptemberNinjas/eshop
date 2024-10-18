using eshop.Core;

namespace eshop.DAL.Json
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в json'е
    /// </summary>
    internal class ProductJsonReadOnlyRepository : JsonRepository<Product>, IReadOnlyRepository<Product>
    {
        private protected override string ResourceFilePath => "data\\products.json";
        
        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll() => (IReadOnlyCollection<Product>)GetItemsFromFile();

        /// <inheritdoc/>
        public Product? GetById(int id) => GetAll().FirstOrDefault(item => item.Id == id);

        /// <inheritdoc/>
        public int GetCount() => GetAll().Count;
    }
}
