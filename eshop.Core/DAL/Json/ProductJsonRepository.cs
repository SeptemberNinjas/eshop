using System.Text.Json;

namespace eshop.Core.DAL.Json
{
    /// <summary>
    /// Реализация репозитория для хранения товаров в json'е
    /// </summary>
    internal class ProductJsonRepository : IRepository<Product>
    {
        /// <inheritdoc/>
        public IReadOnlyCollection<Product> GetAll()
        {
            return (IReadOnlyCollection<Product>)GetProducts();
        }

        /// <inheritdoc/>
        public Product? GetById(int id)
        {
            var products = GetProducts();

            return products.FirstOrDefault(item => item.Id == id);
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            var products = GetProducts();

            return products.Count();
        }

        /// <inheritdoc/>
        public void Insert(Product item)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Product> GetProducts()
        {
            if (!File.Exists("data\\products.json"))
            {
                using var sw = new StreamWriter("data\\products.json");
                sw.WriteLine("[]");
            }
            
            using var sr = new StreamReader("data\\products.json");

            var result = JsonSerializer.Deserialize<IEnumerable<Product>>(sr.BaseStream);

            return (IReadOnlyCollection<Product>)(result ?? []);
        }
    }
}
