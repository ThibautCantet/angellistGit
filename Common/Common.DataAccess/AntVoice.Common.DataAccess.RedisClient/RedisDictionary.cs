using AntVoice.Common.DataAccess.RedisClient.Client;
using AntVoice.Common.DataAccess.RedisClient.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient
{
    [DebuggerDisplay("Count = {Count}"), HostProtection(Synchronization = true, ExternalThreading = true)]
    public class RedisDictionary<TKey, TValue> where TKey : IConvertible
    {
        #region Properties

		private string _namedDictionary;
		private string _hashID;
		private RedisCacheAccess _cacheAccess;

		#endregion

		#region Constructors

		public RedisDictionary(string name)
		{
			_namedDictionary = name;
			_hashID = name.ToString();

			_cacheAccess = new RedisCacheAccess();
		}

        public RedisDictionary(string name, params int[] id)
		{
			_namedDictionary = name;
			_hashID = name.ToString();

			for (int i = 0; i < id.Length; i++)
			{
				_hashID += "/" + id[i].ToString();
			}

			_cacheAccess = new RedisCacheAccess();
		}

		#endregion

        /// <summary>
        /// Careful - This method returns all the values from an hash.
        /// Should only be used from admin
        /// </summary>
        /// <returns></returns>
		public Dictionary<TKey, TValue> GetCompleteDictionary()
		{
			return _cacheAccess.GetAllValues<TKey, TValue>(_namedDictionary, _hashID);
		}

		/// <summary>
		/// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The object to use as the key of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		///   </exception>
		///   
		/// <exception cref="T:System.ArgumentException">
		/// An element with the same key already exists in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		///   </exception>
		public void Add(TKey key, TValue value)
		{
			_cacheAccess.SetHash<TKey, TValue>(_namedDictionary, _hashID, key, value);
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		///   </exception>
		public bool ContainsKey(TKey key)
		{
			return _cacheAccess.HashContainsKey<TKey, TValue>(_namedDictionary, _hashID, key);
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the keys of the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		///   </returns>
		public ICollection<TKey> Keys
		{
			get
			{
				return _cacheAccess.GetKeysFromHash<TKey, TValue>(_namedDictionary, _hashID);
			}
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		///   </exception>
		public bool Remove(TKey key)
		{
			return _cacheAccess.RemoveFromHash<TKey, TValue>(_namedDictionary, _hashID, key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		///   </exception>
		public bool TryGetValue(TKey key, out TValue value)
		{
			value = _cacheAccess.GetFromHash<TKey, TValue>(_namedDictionary, _hashID, key);

			return !object.Equals(value, default(TValue));
		}

		/// <summary>
		/// Gets an <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.ICollection`1"/> containing the values in the object that implements <see cref="T:System.Collections.Generic.IDictionary`2"/>.
		///   </returns>
		public ICollection<TValue> Values
		{
			get
			{
				return _cacheAccess.GetValuesFromHash<TKey, TValue>(_namedDictionary, _hashID);
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <returns>
		/// The element with the specified key.
		///   </returns>
		///   
		/// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is null.
		///   </exception>
		///   
		/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">
		/// The property is retrieved and <paramref name="key"/> is not found.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The property is set and the <see cref="T:System.Collections.Generic.IDictionary`2"/> is read-only.
		///   </exception>
		public TValue this[TKey key]
		{
			get
			{
				return _cacheAccess.GetFromHash<TKey, TValue>(_namedDictionary, _hashID, key);
			}
			set
			{
				_cacheAccess.SetHash<TKey, TValue>(_namedDictionary, _hashID, key, value);
			}
		}

		public List<TValue> GetSomeValues(List<TKey> keys)
		{
			return _cacheAccess.GetSomeValuesFromHash<TKey, TValue>(_namedDictionary, _hashID, keys);
		}

		public void Add(KeyValuePair<TKey, TValue> item)
		{
			_cacheAccess.SetHash<TKey, TValue>(_namedDictionary, _hashID, item.Key, item.Value);
		}

		public void Clear()
		{
			_cacheAccess.Remove(_namedDictionary, _hashID);
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			return ContainsKey(item.Key);
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get
			{
				return _cacheAccess.GetHashCount<TKey, TValue>(_namedDictionary, _hashID);
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			return Remove(item.Key);
		}

		public TValue Set(TKey key, Func<TValue, TValue> action)
		{
			TValue data = _cacheAccess.GetFromHash<TKey, TValue>(_namedDictionary, _hashID, key);
			if (!object.Equals(data, default(TValue)))
			{
				data = action(data);
				_cacheAccess.SetHash<TKey, TValue>(_namedDictionary, _hashID, key, data);
			}
			return data;
		}

		public void AddRange(Dictionary<TKey, TValue> data)
		{
			_cacheAccess.SetRangeInHash<TKey, TValue>(_namedDictionary, _hashID, data);
		}

		public long IncrementKey(TKey key, int incrementBy)
		{
			return _cacheAccess.IncrementInHash<TKey, TValue>(_namedDictionary, _hashID, key, incrementBy);
		}
    }
}
