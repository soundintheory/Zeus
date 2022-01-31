using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeus.Persistence;
using System.Web.Caching;

namespace Zeus.Web.Caching
{
    /// <summary>
    /// Store regularly-accessed zeus items in a cache to avoid repeated persister calls
    /// </summary>
    public class CacheService
    {
        private readonly IPersister _persister;

        private readonly IFinder _finder;

        private readonly Cache _cache;

        private readonly CacheDependencyManager _dependencies;

        public CacheDependencyManager Dependencies => _dependencies;

        public CacheService(IPersister persister, IFinder finder, IWebContext context)
        {
            _persister = persister;
            _finder = finder;
            _cache = context.HttpContext.Cache;
            _dependencies = new CacheDependencyManager();

            _persister.ItemSaving += OnPersisterItemSaving;
            _persister.ItemDeleting += OnPersisterItemDeleting;
        }

        public ContentItem GetItem(int id)
        {
            return _cache.GetOrAdd(CacheKey(id), () =>
            {
                var item = _persister.Get(id);
                return new CacheEntry<ContentItem>(item, _dependencies.ForItem(id));
            });
        }

        public T GetItem<T>(int id) where T : ContentItem
        {
            return _cache.GetOrAdd(CacheKey(id), () =>
            {
                var item = _persister.Get<T>(id);
                return new CacheEntry<T>(item, _dependencies.ForItem(id));
            });
        }

        public ContentItem GetStaticItem(int id)
        {
            var item = GetItem(id);

            // Cache the ID against the type for future reference
            if (item != null)
            {
                UpdateStaticTypeCache(id, item.GetType());
            }

            return item;
        }

        public T GetStaticItem<T>(int id) where T : ContentItem
        {
            var item = GetItem<T>(id);

            // Cache the ID against the type for future reference
            if (item != null)
            {
                UpdateStaticTypeCache(id, typeof(T));
            }

            return item;
        }

        public T GetStaticItem<T>() where T : ContentItem
        {
            var itemId = GetFirstOfTypeID<T>(out var item);

            if (item == default && itemId > 0)
            {
                return GetItem<T>(itemId);
            }

            return item;
        }

        public T GetOrAdd<T>(int itemId, string key, Func<T> factory)
        {
            return _cache.GetOrAdd(CacheKey(itemId, key), () =>
            {
                var value = factory();
                return new CacheEntry<T>(value, _dependencies.ForItem(itemId));
            });
        }

        public int GetFirstOfTypeID<T>() where T : ContentItem => GetFirstOfTypeID(out T _);

        public int GetFirstOfTypeID<T>(out T item) where T : ContentItem
        {
            T foundItem = default;

            var id = _cache.GetOrAdd(CacheKey(typeof(T)), () =>
            {
                foundItem = _finder.QueryItems<T>().FirstOrDefault();
                return new CacheEntry<int>(foundItem.ID, _dependencies.ForItem(foundItem.ID));
            });

            item = foundItem;

            return id;
        }

        public void Invalidate(int id)
        {
            _dependencies.InvalidateItem(id);
        }

        public void Invalidate(ContentItem item)
        {
            if (item != null)
            {
                Invalidate(item.ID);
            }
        }

        private void UpdateStaticTypeCache(int id, Type type)
        {
            var typeKey = CacheKey(type);

            if (!_cache.TryGet<int>(typeKey, out var typeIdValue) || typeIdValue != id)
            {
                _cache.Insert(typeKey, id, _dependencies.ForItem(id));
            }
        }

        private string CacheKey(int id, string key = null)
        {
            if (!string.IsNullOrEmpty(key))
            {
                return $"zeusItems.{id}.{key}";
            }
            return $"zeusItems.{id}";
        }

        private string CacheKey(Type type)
        {
            return $"zeusItems.static.{type.FullName}";
        }

        private void OnPersisterItemSaving(object sender, CancelItemEventArgs e)
        {
            Invalidate(e.AffectedItem);
        }

        private void OnPersisterItemDeleting(object sender, CancelItemEventArgs e)
        {
            Invalidate(e.AffectedItem);
        }
    }

    public static class CacheExtensions
    {
        private readonly static ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        public static bool TryGet<T>(this Cache cache, string key, out T value)
        {
            var output = cache[key];

            if (output != null)
            {
                value = (T)output;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Thread safe way of adding an item to the cache, where the factory method may take some time to complete
        /// </summary>
        public static T GetOrAdd<T>(this Cache cache, string key, Func<T> factory)
        {
            var output = cache[key];

            if (output == null)
            {
                var lockObject = locks.GetOrAdd(key, new object());
                lock (lockObject)
                {
                    output = cache[key];

                    if (output == null)
                    {
                        output = factory();
                        cache[key] = output;
                    }
                }
            }

            return (T)output;
        }

        public static T GetOrAdd<T>(this Cache cache, string key, Func<CacheEntry<T>> factory)
        {
            var output = cache[key];

            if (output == null)
            {
                var lockObject = locks.GetOrAdd(key, new object());
                lock (lockObject)
                {
                    output = cache[key];

                    if (output == null)
                    {
                        var entry = factory();
                        output = entry.Value;
                        cache.Insert(key, entry.Value, entry.Dependencies);
                    }
                }
            }

            return (T)output;
        }
    }

    public class CacheEntry<T>
    {
        public T Value { get; set; }

        public CacheDependency Dependencies { get; set; }

        public CacheEntry(T value, CacheDependency dependencies)
        {
            Value = value;
            Dependencies = dependencies;
        }
    }
}
