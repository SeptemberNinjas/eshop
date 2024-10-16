using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eshop.Core.DAL.Json
{
    /// <summary>
    /// Реализация репозитория для хранения услуг в json'е
    /// </summary>
    internal class ServiceInJsonRepository : IRepository<Service>
    {
        /// <inheritdoc/>
        public IReadOnlyCollection<Service> GetAll()
        {
            return (IReadOnlyCollection<Service>)GetProducts();
        }

        /// <inheritdoc/>
        public Service? GetById(int id)
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
        public void Insert(Service item)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<Service> GetProducts()
        {
            if (!File.Exists("products.json"))
            {
                using var sw = new StreamWriter("products.json");
                sw.WriteLine("[]");
            }

            using var sr = new StreamReader("products.json");

            var result = JsonSerializer.Deserialize<IEnumerable<Product>>(sr.BaseStream);

            return (IReadOnlyCollection<Service>)(result ?? []);
        }
    }
}
