using eshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eshop.DAL.JSON
{
    internal class ProductJsonRepository : IRepository<Product>
    {
        public IReadOnlyCollection<Product> GetAll()
        {
            return (IReadOnlyCollection<Product>)GetProducts();
        }

        public Product? GetById(int id)
        {
            var products = GetProducts();
            return products.FirstOrDefault(item => item.Id == id);
        }

        public int GetCount()
        {
            var products = GetProducts();
            return products.Count();
        }

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
