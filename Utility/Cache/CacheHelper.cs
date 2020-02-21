using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;


namespace Utility
{
    public class CacheHelper  
    {
       
        private static IMemoryCache cache = GlobalContext.ServiceProvider.GetService<IMemoryCache>();

        public static void AddCache<T>(string cacheKey, T value)
        {
            cache.Set(cacheKey, value, DateTimeOffset.Now.AddMinutes(10));
        }

        public static void AddCache<T>(string cacheKey, T value, DateTime expireTime)
        {
            cache.Set(cacheKey, value, expireTime);
        }

        public static void RemoveCache(string cacheKey)
        {
            cache.Remove(cacheKey);
        }

        public static T GetCache<T>(string cacheKey)
        {
            if (cache.Get(cacheKey) != null)
            {
                return (T)cache.Get(cacheKey);
            }
            else
            {
                return default(T);
            }
        }
    }
}
