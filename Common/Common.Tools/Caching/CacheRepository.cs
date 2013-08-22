using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Caching
{
    /// <summary>
    /// The cache repository is the generic class to handle and store cached objects.
    /// It's a factory to get and retrieve cached objects. 
    /// By the GetItem method it will return the cached object if it already exists 
    /// in the cache store and has not expired. If object is not availabe, it will store
    /// a new one by calling the delegate method.
    /// </summary>
    public static class CacheRepository
    {
        /// <summary>
        /// return a cached object
        /// </summary>
        /// <typeparam name="IResponseType">type of the object to return</typeparam>
        /// <param name="fromDataAccessMethod">generic delegate() to call if object does not exists or cache not active</param>
        /// <param name="expirationInMinutes">expiration in minutes</param>
        /// <param name="keys">a string[] of keys</param>
        /// <returns>the cached object of type T</returns>
        public static IResponseType GetItem<IResponseType>(Func<IResponseType> fromDataAccessMethod, int expirationInMinutes, string mainKey, params string[] keys)
        {
            // create a new cache item object from factory based on expiration and keys
            ICacheItem item = CacheItemFactory.Item(mainKey, keys);

            // if default cache manager does not contains the key (the signature of cache item based
            // on cache protocol), 
            if (!MemoryCacheAccess.Current.Contains(item))
            {
                // add a new one with this signature (item.Key) with the value handled from
                // delegate method
                MemoryCacheAccess.Current.AddData(item, fromDataAccessMethod(), expirationInMinutes);
            }

            // in all the cases, return object from the cache
            return (IResponseType)MemoryCacheAccess.Current[item];
        }
        /// <summary>
        /// return a cached object
        /// </summary>
        /// <typeparam name="IResponseType">type of the object to return</typeparam>
        /// <param name="fromDataAccessMethod">generic delegate() to call if object does not exists or cache not active</param>
        /// <param name="expirationInSeconds">expiration in seconds</param>
        /// <param name="keys">a string[] of keys</param>
        /// <returns>the cached object of type T</returns>
        public static IResponseType GetItemSeconds<IResponseType>(Func<IResponseType> fromDataAccessMethod, int expirationInSeconds, string mainKey, params string[] keys)
        {
            // create a new cache item object from factory based on expiration and keys
            ICacheItem item = CacheItemFactory.Item(mainKey, keys);

            // if default cache manager does not contains the key (the signature of cache item based
            // on cache protocol), 
            if (!MemoryCacheAccess.Current.Contains(item))
            {
                // add a new one with this signature (item.Key) with the value handled from
                // delegate method
                MemoryCacheAccess.Current.AddDataSeconds(item, fromDataAccessMethod(), expirationInSeconds);
            }

            // in all the cases, return object from the cache
            return (IResponseType)MemoryCacheAccess.Current[item];
        }

        /// <summary>
        /// Adds an object into the cache
        /// </summary>
        /// <typeparam name="T">type of the object to add</typeparam>
        /// <param name="expirationInMinute">expiration in minutes</param>
        /// <param name="keys">a string[] of keys</param>
        public static void Add<T>(string mainKey, T value, int expirationInMinute, params string[] keys)
        {
            // create a new cache item object from factory based on expiration and keys
            ICacheItem item = CacheItemFactory.Item(mainKey, keys);

            // add a new one with this signature (item.Key) with the value handled from
            // delegate method
            MemoryCacheAccess.Current.AddData(item, value, expirationInMinute);
        }

        /// <summary>
        /// Gets an object from the cache
        /// </summary>
        /// <typeparam name="T">type of the object to return</typeparam>
        /// <param name="keys">a string[] of keys</param>
        public static T Get<T>(string mainKey, params string[] keys)
        {
            // create a new cache item object from factory based on expiration and keys
            ICacheItem item = CacheItemFactory.Item(mainKey, keys);

            // return an object from the cache
            return (T)MemoryCacheAccess.Current[item];
        }

        /// <summary>
        /// Tells if an object exists in cache
        /// </summary>
        /// <param name="keys">a string[] of keys</param>
        public static bool Contains(string mainKey, params string[] keys)
        {
            // create a new cache item object from factory based on expiration and keys
            ICacheItem item = CacheItemFactory.Item(mainKey, keys);

            return MemoryCacheAccess.Current.Contains(item);
        }
    }
}
