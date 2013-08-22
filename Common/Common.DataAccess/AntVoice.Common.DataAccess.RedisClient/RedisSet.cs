using AntVoice.Common.DataAccess.RedisClient.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient
{
    public class RedisSet<TValue> where TValue : IConvertible
    {
        #region Properties

        private string _namedDictionary;
        private string _setName;
        private RedisCacheAccess _cacheAccess;

        #endregion

        public RedisSet(string workspace) : this(workspace, workspace) { }

        public RedisSet(string workspace, string setName)
        {
            _namedDictionary = workspace;
            _setName = setName;
            _cacheAccess = new RedisCacheAccess();
        }

        public bool IsSetExists()
        {
            return _cacheAccess.ContainsKey<TValue>(_namedDictionary, _setName);
        }

        public void Add(TValue value)
        {
            _cacheAccess.AddInSet<TValue>(_namedDictionary, _setName, value);
        }

        public void AddRange(IEnumerable<TValue> values)
        {
            _cacheAccess.AddRangeInSet<TValue>(_namedDictionary, _setName, values);
        }

        public HashSet<TValue> GetRandomItems(int nbValues, StringParserHandler<TValue> parser)
        {
            HashSet<TValue> result = new HashSet<TValue>();
            HashSet<string> values = _cacheAccess.GetRandomKeysFromSet(_namedDictionary, _setName, nbValues);
            foreach (string item in values)
            {
                result.Add(parser(item));
            }
            return result;
        }

        public void Remove(TValue value)
        {
            _cacheAccess.RemoveInSet<TValue>(_namedDictionary, _setName, value);
        }


    }
}
