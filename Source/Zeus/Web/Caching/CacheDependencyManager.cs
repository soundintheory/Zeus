using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace Zeus.Web.Caching
{
    public class CacheDependencyManager
    {
        private ConcurrentDictionary<string, Lazy<ConcurrentStack<ExplicitCacheDependency>>> _dependencies = new ConcurrentDictionary<string, Lazy<ConcurrentStack<ExplicitCacheDependency>>>();

        public CacheDependency Get(string key)
        {
            var list = _dependencies.GetOrAdd(key, new Lazy<ConcurrentStack<ExplicitCacheDependency>>(() => new ConcurrentStack<ExplicitCacheDependency>())).Value;
            var dependency = new ExplicitCacheDependency(key);
            list.Push(dependency);
            return dependency;
        }

        public CacheDependency ForItem(params int[] ids)
        {
            if (ids.Length == 1)
            {
                return Get(ItemKey(ids[0]));
            }

            var aggregateDependency = new AggregateCacheDependency();
            aggregateDependency.Add(ids.Select(x => Get(ItemKey(x))).ToArray());
            return aggregateDependency;
        }

        public CacheDependency ForItem(ContentItem item)
        {
            return Get(ItemKey(item.ID));
        }

        public void Invalidate(string key)
        {
            if (_dependencies.TryGetValue(key, out var lazyList))
            {
                var dependencyList = lazyList.Value;

                lock (dependencyList)
                {
                    foreach (var dependency in dependencyList)
                    {
                        dependency.Invalidate();
                        dependency.Dispose();
                    }

                    dependencyList.Clear();
                }
            }
        }
        public void InvalidateItem(int id)
        {
            Invalidate(ItemKey(id));
        }

        public void InvalidateItem(ContentItem item)
        {
            Invalidate(ItemKey(item.ID));
        }

        private string ItemKey(int id) => $"zeusItem_{id}";
    }
}
