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
            return (IReadOnlyCollection<Service>)GetServices();
        }

        /// <inheritdoc/>
        public Service? GetById(int id)
        {
            var products = GetServices();

            return products.FirstOrDefault(item => item.Id == id);
        }

        /// <inheritdoc/>
        public int GetCount()
        {
            var products = GetServices();

            return products.Count();
        }

        /// <inheritdoc/>
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
