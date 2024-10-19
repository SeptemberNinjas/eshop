using eshop.Core;

namespace eshop.DAL.Json
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в json'е
    /// </summary>
    internal class ProductJsonReadOnlyRepository : JsonRepository<Product>, IRepository<Product>, IRepository<SaleItem>
    {
        private protected override string ResourceFilePath => "data\\products.json";
        
        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll() => GetItemsFromFile().ToArray();

        /// <inheritdoc/>
        public Product? GetById(int id) => GetAll().FirstOrDefault(item => item.Id == id);

        IReadOnlyCollection<SaleItem> IReadOnlyRepository<SaleItem>.GetAll()
        {
            return GetAll();
        }

        /// <inheritdoc cref="IReadOnlyRepository{T}.GetCount" />
        public int GetCount() => GetAll().Count;

        SaleItem? IReadOnlyRepository<SaleItem>.GetById(int id)
        {
            return GetById(id);
        }

        public void Update(Product item)
        {
            var items = GetItemsFromFile();
            items = items.Select(i => i.Id == item.Id
                ? item: i); 
            SaveItemsToFile(items);
        }

        public int Insert(Product item)
        {
            throw new NotSupportedException("Добавление новых товаров не поддерживается");
        }

        public void Update(SaleItem item)
        {
            if (item is Product product)
                Update(product);
        }

        public int Insert(SaleItem item)
        {
            return item is Product product
                ? Insert(product)
                : throw new ArgumentException("Неверный тип репозитория");
        }
    }
}
