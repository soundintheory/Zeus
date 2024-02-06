using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zeus.Web.Caching
{
    public class MemoryCacheStore<TKey,TValue>
    {
        private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _cache = new ConcurrentDictionary<TKey, Lazy<TValue>>();

        public TValue GetOrAdd(TKey key, TValue value)
        {
            return _cache.GetOrAdd(key, new Lazy<TValue>(() => value)).Value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            return _cache.GetOrAdd(key, (_key) => new Lazy<TValue>(() => valueFactory(_key))).Value;
        }

        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return _cache.AddOrUpdate(
                key,
                (_key) => new Lazy<TValue>(() => addValueFactory(_key)),
                (_key, currentValue) => new Lazy<TValue>(() => updateValueFactory(_key, currentValue.Value))
            ).Value;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                value = entry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            if (_cache.TryRemove(key, out var entry))
            {
                value = entry.Value;
                return true;
            }
            value = default;
            return false;
        }

    }
}
