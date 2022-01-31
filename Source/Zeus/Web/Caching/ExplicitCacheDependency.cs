using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Zeus.Web.Caching
{
    /// <summary>
    /// A type of cache dependency that is stored in memory and can be explicitly invalidated by code,
    /// so no DB or filesystem checks are required
    /// </summary>
    public class ExplicitCacheDependency : CacheDependency
    {
        private string _uniqueId;

        public ExplicitCacheDependency(string uniqueId) : base(new string[0]) // no file system dependencies
        {
            _uniqueId = uniqueId;
        }

        public override string GetUniqueID()
        {
            return _uniqueId;
        }

        public void Invalidate()
        {
            NotifyDependencyChanged(this, EventArgs.Empty);
        }
    }
}
