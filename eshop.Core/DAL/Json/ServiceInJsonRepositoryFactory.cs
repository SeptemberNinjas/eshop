using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.Core.DAL.Json
{
    /// <summary>
    /// Реализация фабрики для хранения товаров в json'е
    /// </summary>
    public class ServiceInJsonRepositoryFactory : RepositoyFactory<Service>
    {
        public override IRepository<Service> Create()
        {
            return new ServiceInJsonRepository();
        }

    }
}
