using eshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.DAL.Memory
{
    public class ProductMemoryRepository : IRepository<Product>
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

        public IReadOnlyCollection<Product> GetAll()
        {
            return _products.AsReadOnly();
        }

        public int GetCount()
        {
            return _products.Count;
        }

        public Product? GetById(int id)
        {
            return _products.FirstOrDefault(item => item.Id == id);
        }

        public void Insert(Product item)
        {
            _products.Add(item);
        }

    }
}
