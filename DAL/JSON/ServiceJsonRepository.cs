using eshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace eshop.DAL.JSON
{
    internal class ServiceJsonRepository : IRepository<Service>
    {
        public IReadOnlyCollection<Service> GetAll()
        {
            return (IReadOnlyCollection<Service>)GetServices();
        }

        public Service? GetById(int id)
        {
            var products = GetServices();

            return products.FirstOrDefault(item => item.Id == id);
        }

        public int GetCount()
        {
            var products = GetServices();

            return products.Count();
        }

        public void Insert(Service item)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable<Service> GetServices()
        {
            if (!File.Exists("data\\services.json"))
            {
                using var sw = new StreamWriter("data\\services.json");
                sw.WriteLine("[]");
            }

            using var sr = new StreamReader("data\\services.json");

            var result = JsonSerializer.Deserialize<IEnumerable<Service>>(sr.BaseStream);

            return (IReadOnlyCollection<Service>)(result ?? []);
        }
    }
}
