using eshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.DAL.Memory
{
    public class ServiceMemoryRepository : IRepository<Service>
    {
        private readonly List<Service> _services;

        public ServiceMemoryRepository()
        {
            _services =
            [
                new Service(3, "Раскопать яму", 5.49m),
                new Service(4, "Вспахать поле", 1000)
            ];
        }

        public IReadOnlyCollection<Service> GetAll()
        {
            return _services.AsReadOnly();
        }

        public int GetCount()
        {
            return _services.Count;
        }

        public Service? GetById(int id)
        {
            return _services.FirstOrDefault(item => item.Id == id);
        }
    }
}
