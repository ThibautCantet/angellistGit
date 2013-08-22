using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace Platform.Tools.Caching
{
    internal class MemoryCacheAccess
    {
        private static readonly MemoryCacheAccess _instance = new MemoryCacheAccess();

		private MemoryCacheAccess()
		{
			// empty constructor
		}

		public static MemoryCacheAccess Current
		{
			get
			{
				return _instance;
			}
		}

		#region Indexer

		public object this[ICacheItem key]
		{
			get { return GetData(key); }
		}
		#endregion

		#region Add data to cache

		public void AddSlidingData(ICacheItem key, object value, int expirationMinutes, CacheEntryRemovedCallback objectExpiredEvent)
		{
            CacheItemPolicy policy = new CacheItemPolicy();
			if (expirationMinutes > 0)
            {
                policy.RemovedCallback = objectExpiredEvent;
                policy.SlidingExpiration = TimeSpan.FromMinutes(expirationMinutes);
            }
            MemoryCache.Default.Add(key.ToString(), value, policy);
		}

		public void AddData(ICacheItem key, object value, int expirationMinutes)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            if (expirationMinutes > 0)
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
            }
            MemoryCache.Default.Add(key.ToString(), value, policy);
		}

		public void AddDataSeconds(ICacheItem key, object value, int expirationSeconds)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            if (expirationSeconds > 0)
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(expirationSeconds);
            }
            MemoryCache.Default.Add(key.ToString(), value, policy);
		}

		public void AddData(ICacheItem key, object value, int expirationMinutes, CacheEntryRemovedCallback objectExpiredEvent)
        {
            CacheItemPolicy policy = new CacheItemPolicy();
            if (expirationMinutes > 0)
            {
                policy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);
                policy.RemovedCallback = objectExpiredEvent;
            }
            MemoryCache.Default.Add(key.ToString(), value, policy);
		}

		#endregion

		#region Get data from cache
        
		public object GetData(ICacheItem key)
		{
			return MemoryCache.Default.Get(key.ToString());
		}

		#endregion

		#region Does cache contain data

		public bool Contains(ICacheItem key)
		{
			return MemoryCache.Default.Contains(key.ToString()) && MemoryCache.Default.Get(key.ToString()) != null;
		}

		#endregion

		#region Flush cache

		/// <summary>
		/// Flush all caches
		/// </summary>
		public void FlushCache()
		{
            foreach (var item in MemoryCache.Default)
            {
                MemoryCache.Default.Remove(item.Key);
            }
		}

		#endregion

		#region Count

		public int GetCount()
		{
			return MemoryCache.Default.Count();
		}

		#endregion
    }
}
