using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.Web.Caching
{
    public class ZeusCacheDependency : ExplicitCacheDependency
    {
        private int _itemId;

        public ZeusCacheDependency(int itemId) : base(GetKey(itemId))
        {
            _itemId = itemId;
        }

        public int ItemID => _itemId;

        public static string GetKey(int itemId) => $"zeusItem_{itemId}";
    }
}
