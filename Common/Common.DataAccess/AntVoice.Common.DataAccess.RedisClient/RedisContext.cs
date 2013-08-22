using AntVoice.Common.DataAccess.RedisClient.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient
{
    /// <summary>
    /// This class is used to get access to Redis basic structures
    /// You can set keys with values not using complex structures
    /// </summary>
    /// <typeparam name="TKey">The type of the keys</typeparam>
    /// <typeparam name="TValue">The type of the values</typeparam>
    [HostProtection(Synchronization = true, ExternalThreading = true)]
    public class RedisContext<TKey, TValue>
        where TKey : IConvertible
    {
        #region Properties

        private string _workspace;
        private string _prefix;
        private RedisCacheAccess _cacheAccess;

        #endregion

        #region Constructors

        public RedisContext(string workspace)
        {
            _workspace = workspace;
            _prefix = workspace;

            _cacheAccess = new RedisCacheAccess();
        }

		public RedisContext(string workspace, params int[] id)
		{
			_workspace = workspace;
			_prefix = workspace;

			for (int i = 0; i < id.Length; i++)
			{
				_prefix += "/" + id[i].ToString();
			}

			_cacheAccess = new RedisCacheAccess();
		}

        #endregion

        #region Cache methods

        public void Set(TKey key, TValue value)
        {
            _cacheAccess.Set<TValue>(_workspace, string.Concat(_prefix, "/", key.ToString()), value);
        }

        public void Set(TKey key, TValue value, int expiresInMinute)
        {
            _cacheAccess.SetExpiresInMinutes<TValue>(_workspace, string.Concat(_prefix, "/", key.ToString()), value, expiresInMinute);
        }

        public void SetAll(IDictionary<TKey, TValue> data)
        {
            Dictionary<string, TValue> values = new Dictionary<string, TValue>();
            foreach (TKey key in data.Keys)
            {
                values[string.Concat(_prefix, "/", key.ToString())] = data[key];
            }
            _cacheAccess.SetAll<TValue>(_workspace, values);
        }

        public void SetAll(IDictionary<TKey, TValue> data, int expiresInMinute)
        {
            Dictionary<string, TValue> values = new Dictionary<string, TValue>();
            foreach (TKey key in data.Keys)
            {
                values[string.Concat(_prefix, "/", key.ToString())] = data[key];
            }
            _cacheAccess.SetAll<TValue>(_workspace, values, expiresInMinute);

        }

        public long Increment(TKey key)
        {
            return Increment(key, 1);
        }

        public long Increment(TKey key, int incrementBy)
        {
            return _cacheAccess.Increment<TKey, TValue>(_workspace, key.ToString(), incrementBy);
        }

		public TValue Get(TKey key)
		{
            return _cacheAccess.Get<TValue>(_workspace, string.Concat(_prefix, "/", key.ToString()));
		}

		public TValue Get(TKey key, out bool foundKey)
		{
            return _cacheAccess.Get<TValue>(_workspace, string.Concat(_prefix, "/", key.ToString()), out foundKey);
		}

        public bool ContainsKey(TKey key)
        {
            return _cacheAccess.ContainsKey<TValue>(_workspace, string.Concat(_prefix, "/", key));
        }

        public bool Remove(TKey key)
        {
            return _cacheAccess.Remove(_workspace, string.Concat(_prefix, "/", key));
        }

        public int Count()
        {
            return _cacheAccess.SearchKeys(_workspace, _prefix).Count;
        }

        public IDictionary<TKey, TValue> GetMultipleValues(IEnumerable<string> keys, StringParserHandler<TKey> parser)
        {
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            List<string> allKeys = new List<string>();
            foreach (string key in keys)
            {
                allKeys.Add(string.Concat(_prefix, "/", key));
            }
            return _cacheAccess.GetSomeValues<TKey, TValue>(_workspace, allKeys, parser);
        }

        public void ClearKeys()
        {
            List<string> keys = _cacheAccess.SearchKeys(_workspace, string.Concat(_prefix, "/*"));
            _cacheAccess.RemoveAll(_workspace, keys);
        }

        #endregion
    }
}
