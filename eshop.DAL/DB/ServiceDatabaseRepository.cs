using eshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.DAL.DB
{
    internal class ServiceDatabaseRepository : DatabaseContext, IRepository<Service>
    {
        public ServiceDatabaseRepository(string connectionString) : base(connectionString)
        {
        }

        public IReadOnlyCollection<Service> GetAll()
        {
            throw new NotImplementedException();
        }

        public Service? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public int GetCount()
        {
            throw new NotImplementedException();
        }

        public int Insert(Service item)
        {
            throw new NotImplementedException();
        }

        public void Update(Service item)
        {
            throw new NotImplementedException();
        }
    }
}
