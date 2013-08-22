using AntVoice.Common.DataAccess.RedisClient.Configuration;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using log4net;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient.Client
{
    internal class RedisClientPool
    {
        #region Properties

        private Dictionary<string, HostSettings> _hosts = new Dictionary<string, HostSettings>();
        private Dictionary<string, HostSettings> _specificHosts = new Dictionary<string, HostSettings>();

        private Dictionary<string, PooledRedisClientManager> _pools = new Dictionary<string, PooledRedisClientManager>();
        private Logger _logger = Logger.Current;

        #endregion

        #region Constructors

        public RedisClientPool()
        {
            var configuration = CacheConfiguration.Current.Redis;

            foreach (HostSettings host in configuration.Hosts)
            {
                _hosts[host.Name] = host;

                _pools[host.Name] = new PooledRedisClientManager(
                    new List<string>() { host.URL + ":" + host.Port },
                    null,
                    new RedisClientManagerConfig
                    {
                        MaxWritePoolSize = host.MaxClients,
                        MaxReadPoolSize = host.MaxClients
                    });
            }

            foreach (DictionarySettings dictionary in configuration.Dictionaries)
            {
                _specificHosts[dictionary.Name] = _hosts[dictionary.Host];
            }
        }

        #endregion

        #region ICacheClientPool Members

        public IRedisClient GetClient(string name)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                HostSettings host;
                if (_specificHosts.ContainsKey(name))
                {
                    host = _specificHosts[name];
                }
                else
                {
                    host = _hosts["default"];
                }

                tm.Start();
                return _pools[host.Name].GetClient();
            }
            catch (Exception e)
            {
                _logger.Error("RedisClientPool.GetRedisClient", e.Message, e);
            }
            finally
            {
                tm.Stop();
                MonitoringTimers.Current.AddTime(Counters.Redis_GetClient, tm);
            }
            return null;
        }

        public void ReleaseClient(string name, IRedisClient client)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                client.Dispose();
            }
            catch (Exception e)
            {
                _logger.Error("RedisClientPool.ReleaseClient", e.Message, e);
            }
            finally
            {
                tm.Stop();
                MonitoringTimers.Current.AddTime(Counters.Redis_ReleaseClient, tm);
            }
        }

        public void ClearAllKeys()
        {
            foreach (HostSettings host in _hosts.Values)
            {
                using (IRedisClient client = new ServiceStack.Redis.RedisClient(host.URL, host.Port))
                {
                    client.FlushAll();
                }
            }
        }

        #endregion
    }
}
