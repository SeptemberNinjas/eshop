using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eshop.Core
{
    public interface IRepository<T>
    {
        IReadOnlyCollection<T> GetAll();
        int GetCount();
        T? GetById(int id);
    }
}
