using System.Collections.Generic;
using System.Web;

namespace ioschools.Caching
{
    public sealed class CacheHelper 
    {
        /// <summary>
        /// dependency map key is the dependency type and the hashset is keys for all cached items
        /// </summary>
        private readonly Dictionary<string, HashSet<string>> dep_map;
        public static readonly CacheHelper Instance = new CacheHelper();
        private CacheHelper()
        {
            dep_map = new Dictionary<string, HashSet<string>>();
        }

        private static string CreateCacheItemKey(CacheItemType type, string key)
        {
            return type + "_res:" + key;
        }

        private static string CreateDependencyKey(DependencyType type, string key)
        {
            return type + "_deps:" + key;
        }

        public bool TryGetCache(CacheItemType type, string key, out object value)
        {
            string cachekey = CreateCacheItemKey(type, key);
            
            // anything in cache
            object data = HttpRuntime.Cache.Get(cachekey);

            // yes, return cached entry
            if (data == null)
            {
                value = null;
                return false;
            }
            value = data;
            return true;
        }

        public string Insert(CacheItemType type, string key, object value)
        {
            if (value == null)
            {
                return null;
            }

            string cacheKey = CreateCacheItemKey(type, key);
            // add with no sliding or absolute expiration
            // cache dependencies are manually handled
            HttpRuntime.Cache.Insert(cacheKey,value);

            return cacheKey;
        }

        public void add_dependency(DependencyType depType, string depid, CacheItemType itemType, string itemid)
        {
            // if null then just add independent cache entry
            string cacheKey = CreateCacheItemKey(itemType, itemid);
            string dependencyKey = CreateDependencyKey(depType, depid);
            if (!dep_map.ContainsKey(dependencyKey))
            {
                dep_map.Add(dependencyKey, new HashSet<string>());
            }
            dep_map[dependencyKey].Add(cacheKey);
        }

        public void invalidate_dependency(DependencyType type, string key)
        {
            string dependencyKey = CreateDependencyKey(type, key);
            if (!dep_map.ContainsKey(dependencyKey))
            {
                return;
            }
            // remove affected cached entries
            HashSet<string> cachekeys = dep_map[dependencyKey];
            foreach (var cachekey in cachekeys)
            {
                HttpRuntime.Cache.Remove(cachekey);
            }
            // remove dependency
            dep_map.Remove(dependencyKey);
        }
    }
}