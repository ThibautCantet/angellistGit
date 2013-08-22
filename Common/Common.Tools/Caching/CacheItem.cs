using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Caching
{
    #region Cache Item classes

    /// <summary>
    /// generic contract for cached items
    /// </summary>
    internal interface ICacheItem
    {
        string Key
        {
            get;
            set;
        }
    }

    /// <summary>
    /// single named cache item (without parameter).
    /// Expiration on constructor is the key in resources in database
    /// that defines the expiring time for this kind of key.
    /// </summary>
    internal class CacheItemSingle : ICacheItem
    {
        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public CacheItemSingle()
        {
            Key = CacheProtocol.GetURI();
        }

        public CacheItemSingle(int _expiration, string _mainKey)
        {
            key = CacheProtocol.GetURI(_mainKey);
        }

        public override string ToString()
        {
            return key;
        }
    }

    /// <summary>
    /// multi parameter cached item
    /// </summary>
    internal class CacheItemMulti : ICacheItem
    {
        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public CacheItemMulti()
        {
            Key = CacheProtocol.GetURI();
        }

        public CacheItemMulti(string _mainKey, params string[] _keys)
        {
            key = CacheProtocol.GetURI(_mainKey, _keys);
        }

        public override string ToString()
        {
            return key;
        }
    }

    /// <summary>
    /// build cache items on demand
    /// </summary>
    internal static class CacheItemFactory
    {
        public static ICacheItem Item(string _mainkey)
        {
            return new CacheItemSingle(0, _mainkey);
        }

        public static ICacheItem Item(string _mainKey, params string[] _keys)
        {
            return new CacheItemMulti(_mainKey, _keys);
        }
    }

    #endregion
}
