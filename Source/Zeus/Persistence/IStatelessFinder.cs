using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.Persistence
{
    public interface IStatelessFinder : IFinder, IDisposable
    {
        ITransaction BeginTransaction();
        TKey Insert<TKey>(object entity);
        void Update(object entity);
        void Delete(object entity);
    }
}
