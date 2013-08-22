using AntVoice.Common.DataAccess.RedisClient.Tools;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using ServiceStack.Redis;
using ServiceStack.Redis.Pipeline;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient.Client
{
    public delegate TKey StringParserHandler<TKey>(string value);

    internal class RedisCacheAccess
    {
        private readonly int NUM_RETRIES = 3;
        private readonly int RETRY_TIMEOUT = 100;
        
        #region Set operations

        public void Set<T>(string name, string key, T value)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                    () =>
                    {
                        IRedisClient _client = GetRedisClient(name);
                        try
                        {
                            tm.Start();
                            _client.Set<T>(key, value);
                        }
                        finally
                        {
                            ReleaseRedisClient(name, _client);
                            tm.Stop();
                            MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                        }
                    });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key, e);
            }
        }

        public void SetAll<T>(string name, IDictionary<string, T> data)
        {
            if (data.Count > 0)
            {
                IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                try
                {
                    RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                    () =>
                    {
                        IRedisClient _client = GetRedisClient(name);
                        try
                        {
                            tm.Start();
                            _client.SetAll<T>(data);
                        }
                        finally
                        {
                            ReleaseRedisClient(name, _client);
                            tm.Stop();
                            MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                        }
                    });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", "SetAll", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", "SetAll", e);
                }
            }
        }

        public void SetAll<T>(string name, IDictionary<string, T> data, int expiresInMinute)
        {
            if (data.Count > 0)
            {
                IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                try
                {
                    RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                    () =>
                    {
                        IRedisClient _client = GetRedisClient(name);
                        try
                        {
                            tm.Start();

                            TimeSpan expiresIn = TimeSpan.FromMinutes(expiresInMinute);
                            using (IRedisPipeline pipe = _client.CreatePipeline())
                            {
                                foreach (string key in data.Keys)
                                {
                                    pipe.QueueCommand(c => c.Set(key, data[key], expiresIn));
                                }

                                pipe.Flush();
                            }
                        }
                        finally
                        {
                            ReleaseRedisClient(name, _client);
                            tm.Stop();
                            MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                        }
                    });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", "SetAll", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", "SetAll", e);
                }
            }
        }

        public void SetExpiresInSeconds<T>(string name, string key, T value, int expiresInSeconds)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                       () =>
                       {
                           IRedisClient _client = GetRedisClient(name);
                           try
                           {
                               tm.Start();
                               _client.Set<T>(key, value, TimeSpan.FromSeconds(expiresInSeconds));
                           }
                           finally
                           {
                               ReleaseRedisClient(name, _client);
                               tm.Stop();
                               MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                           }
                       });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key, e);
            }
        }

        public void SetExpiresInMinutes<T>(string name, string key, T value, int expiresInMinutes)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                       () =>
                       {
                           IRedisClient _client = GetRedisClient(name);
                           try
                           {
                               tm.Start();
                               _client.Set<T>(key, value, TimeSpan.FromMinutes(expiresInMinutes));
                           }
                           finally
                           {
                               ReleaseRedisClient(name, _client);
                               tm.Stop();
                               MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                           }
                       });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key, e);
            }
        }

        public void SetExpiresAt<T>(string name, string key, T value, DateTime expiresAt)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                          () =>
                          {
                              IRedisClient _client = GetRedisClient(name);
                              try
                              {
                                  tm.Start();
                                  _client.Set<T>(key, value, expiresAt);
                              }
                              finally
                              {
                                  ReleaseRedisClient(name, _client);
                                  tm.Stop();
                                  MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                              }
                          });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key, e);
            }
        }

        public void SetHash<TKey, TValue>(string name, string hashID, TKey key, TValue value)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                       () =>
                       {
                           IRedisClient _client = GetRedisClient(name);
                           try
                           {
                               var client = _client.GetTypedClient<TValue>();
                               var hash = client.GetHash<TKey>(hashID);

                               tm.Start();
                               client.SetEntryInHash<TKey>(hash, key, value);
                           }
                           finally
                           {
                               ReleaseRedisClient(name, _client);
                               tm.Stop();
                               MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                           }
                       });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
        }

        public void SetRangeInHash<TKey, TValue>(string name, string hashID, IEnumerable<KeyValuePair<TKey, TValue>> range)
        {
            if (range.Count() > 0)
            {
                IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                try
                {
                    RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                       () =>
                       {
                           IRedisClient _client = GetRedisClient(name);
                           try
                           {
                               var client = _client.GetTypedClient<TValue>();
                               var hash = client.GetHash<TKey>(hashID);

                               tm.Start();
                               Dictionary<TKey, TValue> temp = new Dictionary<TKey, TValue>(1000);
                               int currentCount = 0;
                               foreach (KeyValuePair<TKey, TValue> item in range)
                               {
                                   temp[item.Key] = item.Value;

                                   currentCount++;
                                   if (currentCount == 1000)
                                   {
                                       client.SetRangeInHash<TKey>(hash, temp);
                                       currentCount = 0;
                                       temp.Clear();
                                   }
                               }

                               client.SetRangeInHash<TKey>(hash, temp);
                           }
                           finally
                           {
                               ReleaseRedisClient(name, _client);
                               tm.Stop();
                               MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                           }
                       });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "SetRangeInHash", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", hashID, e);
                }
            }
        }

        public long Increment<TKey, TValue>(string name, string key, int incrementBy)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, long>(NUM_RETRIES, RETRY_TIMEOUT,
                          () =>
                          {
                              IRedisClient _client = GetRedisClient(name);
                              try
                              {
                                  tm.Start();
                                  return _client.IncrementValueBy(key, incrementBy);
                              }
                              finally
                              {
                                  ReleaseRedisClient(name, _client);
                                  tm.Stop();
                                  MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                              }
                          });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key.ToString(), e);
            }
            return -1;
        }

        public long Decrement<TKey, TValue>(string name, string key, int decrementBy)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, long>(NUM_RETRIES, RETRY_TIMEOUT,
                             () =>
                             {
                                 IRedisClient _client = GetRedisClient(name);
                                 try
                                 {
                                     tm.Start();
                                     return _client.DecrementValueBy(key, decrementBy);
                                 }
                                 finally
                                 {
                                     ReleaseRedisClient(name, _client);
                                     tm.Stop();
                                     MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                 }
                             });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key.ToString(), e);
            }
            return -1;
        }

        public long IncrementInHash<TKey, TValue>(string name, string hashID, TKey key, int incrementBy)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, long>(NUM_RETRIES, RETRY_TIMEOUT,
                             () =>
                             {
                                 IRedisClient _client = GetRedisClient(name);
                                 try
                                 {
                                     tm.Start();
                                     return _client.IncrementValueInHash(hashID, key.ToString(), incrementBy);
                                 }
                                 finally
                                 {
                                     ReleaseRedisClient(name, _client);
                                     tm.Stop();
                                     MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                 }
                             });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return -1;
        }

        #endregion

        #region Existence

        public bool ContainsKey<T>(string name, string key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                         () =>
                         {
                             IRedisClient _client = GetRedisClient(name);
                             try
                             {
                                 tm.Start();
                                 return _client.ContainsKey(key);
                             }
                             finally
                             {
                                 ReleaseRedisClient(name, _client);
                                 tm.Stop();
                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                             }
                         });
        }

        public bool HashContainsKey<TKey, TValue>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                                () =>
                                {
                                    IRedisClient _client = GetRedisClient(name);
                                    try
                                    {
                                        var client = _client.GetTypedClient<TValue>();
                                        var hash = client.GetHash<TKey>(hashID);

                                        tm.Start();
                                        return client.HashContainsEntry<TKey>(hash, key);
                                    }
                                    finally
                                    {
                                        ReleaseRedisClient(name, _client);
                                        tm.Stop();
                                        MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                    }
                                });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID, e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return false;
        }

        public bool OrderedHashContainsKey<TKey>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                             () =>
                             {
                                 IRedisClient _client = GetRedisClient(name);
                                 try
                                 {
                                     tm.Start();
                                     return _client.SortedSetContainsItem(hashID, key.ToString());
                                 }
                                 finally
                                 {
                                     ReleaseRedisClient(name, _client);
                                     tm.Stop();
                                     MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                 }
                             });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID, e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return false;
        }

        public int GetHashCount<TKey, TValue>(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                () =>
                                {
                                    IRedisClient _client = GetRedisClient(name);
                                    try
                                    {
                                        var client = _client.GetTypedClient<TValue>();
                                        var hash = client.GetHash<TKey>(hashID);

                                        tm.Start();
                                        return client.GetHashCount<TKey>(hash);
                                    }
                                    finally
                                    {
                                        ReleaseRedisClient(name, _client);
                                        tm.Stop();
                                        MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                    }
                                });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID, e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetHashCount", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return 0;
        }

        public int GetOrderedHashCount(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                () =>
                                {
                                    IRedisClient _client = GetRedisClient(name);
                                    try
                                    {
                                        tm.Start();
                                        return _client.GetSortedSetCount(hashID);
                                    }
                                    finally
                                    {
                                        ReleaseRedisClient(name, _client);
                                        tm.Stop();
                                        MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                    }
                                });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID, e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetOrderedHashCount", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return 0;
        }

        #endregion

        #region Get operations

        public T Get<T>(string name, string key)
        {
            bool foundKey;
            return Get<T>(name, key, false, out foundKey);
        }
        public T Get<T>(string name, string key, out bool foundKey)
        {
            return Get<T>(name, key, true, out foundKey);
        }
        private T Get<T>(string name, string key, bool checkContains, out bool foundKey)
        {
            bool found = false;
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);

            try
            {
                T result = RetryUtility.RetryActionOnException<SocketException, T>(NUM_RETRIES, RETRY_TIMEOUT,
                    () =>
                    {
                        IRedisClient _client = GetRedisClient(name);
                        try
                        {
                            tm.Start();

                            if (checkContains)
                            {
                                found = _client.ContainsKey(key);
                                if (found)
                                {
                                    return _client.Get<T>(key);
                                }
                            }
                            else
                            {
                                return _client.Get<T>(key);
                            }
                        }
                        catch (InvalidCastException)
                        {
                            _client.Remove(key);
                        }
                        finally
                        {
                            ReleaseRedisClient(name, _client);
                            tm.Stop();
                            MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                        }

                        return default(T);
                    });

                foundKey = found;
                return result;
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", key, e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key, e);
            }

            foundKey = false;
            return default(T);
        }

        public TValue GetFromHash<TKey, TValue>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, TValue>(NUM_RETRIES, RETRY_TIMEOUT,
                                      () =>
                                      {
                                          IRedisClient _client = GetRedisClient(name);
                                          var client = _client.GetTypedClient<TValue>();
                                          var hash = client.GetHash<TKey>(hashID);
                                          try
                                          {
                                              tm.Start();
                                              return client.GetValueFromHash<TKey>(hash, key);
                                          }
                                          catch (InvalidCastException)
                                          {
                                              client.RemoveEntryFromHash<TKey>(hash, key);
                                          }
                                          finally
                                          {
                                              ReleaseRedisClient(name, _client);
                                              tm.Stop();
                                              MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                          }
                                          return default(TValue);
                                      });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return default(TValue);
        }

        public List<TValue> GetSomeValuesFromHash<TKey, TValue>(string name, string hashID, List<TKey> keys)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, List<TValue>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             var client = _client.GetTypedClient<TValue>();
                                             var hash = client.GetHash<TKey>(hashID);
                                             try
                                             {
                                                 List<TValue> result = new List<TValue>();
                                                 tm.Start();
                                                 foreach (TKey key in keys)
                                                 {
                                                     // result.Add(hash[key]);
                                                     // -> Needed to use GetValueFromHash, else got this under the unit tests:
                                                     //      Tests.Common.Cache.RedisCacheAccessTest.RedisCacheAccess_GetSomeValuesFromHash_ReturnsExpectedValue:
                                                     //      System.InvalidOperationException : You are trying to set an expectation on a property that was defined to use PropertyBehavior.
                                                     //      Instead of writing code such as this: mockObject.Stub(x => x.SomeProperty).Return(42);
                                                     //      You can use the property directly to achieve the same result: mockObject.SomeProperty = 42;
                                                     try
                                                     {
                                                         result.Add(client.GetValueFromHash<TKey>(hash, key));
                                                     }
                                                     catch (InvalidCastException)
                                                     {
                                                         client.RemoveEntryFromHash<TKey>(hash, key);
                                                     }
                                                 }
                                                 result.RemoveAll(v => object.Equals(v, default(TValue)));
                                                 return result;
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID + "/Multiple keys", e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "Multiple keys", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/Multiple keys", e);
            }
            return new List<TValue>();
        }

        public List<string> SearchKeys(string name, string pattern)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, List<string>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 return _client.SearchKeys(pattern).ToList();
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", "SearchKeys", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", "SearchKeys", e);
            }
            return new List<string>();
        }

        public ICollection<TKey> GetKeysFromHash<TKey, TValue>(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, ICollection<TKey>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 var client = _client.GetTypedClient<TValue>();
                                                 var hash = client.GetHash<TKey>(hashID);

                                                 tm.Start();
                                                 return client.GetHashKeys<TKey>(hash);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetKeysFromHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return null;
        }

        public ICollection<TValue> GetValuesFromHash<TKey, TValue>(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, ICollection<TValue>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 var client = _client.GetTypedClient<TValue>();
                                                 var hash = client.GetHash<TKey>(hashID);

                                                 tm.Start();
                                                 return client.GetHashValues<TKey>(hash);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetValuesFromHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return null;
        }

        public Dictionary<TKey, TValue> GetAllValues<TKey, TValue>(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, Dictionary<TKey, TValue>>(NUM_RETRIES, RETRY_TIMEOUT,
                                      () =>
                                      {
                                          IRedisClient _client = GetRedisClient(name);
                                          try
                                          {
                                              var client = _client.GetTypedClient<TValue>();
                                              var hash = client.GetHash<TKey>(hashID);

                                              tm.Start();
                                              return client.GetAllEntriesFromHash<TKey>(hash);
                                          }
                                          finally
                                          {
                                              ReleaseRedisClient(name, _client);
                                              tm.Stop();
                                              MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                          }
                                      });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetAllValues", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return null;
        }

        public IDictionary<TKey, TValue> GetSomeValues<TKey, TValue>(string name, IEnumerable<string> keys, StringParserHandler<TKey> parser)
        {
            if (keys.Count() > 0)
            {
                IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);

                try
                {
                    return RetryUtility.RetryActionOnException<SocketException, IDictionary<TKey, TValue>>(NUM_RETRIES, RETRY_TIMEOUT,
                                          () =>
                                          {
                                              Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
                                              IRedisClient _client = GetRedisClient(name);
                                              try
                                              {
                                                  tm.Start();

                                                  IDictionary<string, TValue> data = _client.GetAll<TValue>(keys);
                                                  foreach (string key in data.Keys)
                                                  {
                                                      if (!object.Equals(data[key], default(TValue)))
                                                          result[parser(key)] = data[key];
                                                  }
                                              }
                                              finally
                                              {
                                                  ReleaseRedisClient(name, _client);
                                                  tm.Stop();
                                                  MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                              }
                                              return result;
                                          });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", "GetSomeValues", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", "GetSomeValues", e);
                }
            }
            return new Dictionary<TKey, TValue>();
        }

        #endregion

        #region Remove operations

        public bool Remove(string name, string key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                                     () =>
                                     {
                                         IRedisClient _client = GetRedisClient(name);
                                         try
                                         {
                                             tm.Start();
                                             return _client.Remove(key);
                                         }
                                         finally
                                         {
                                             ReleaseRedisClient(name, _client);
                                             tm.Stop();
                                             MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                         }
                                     });
        }

        public void RemoveAll(string name, IEnumerable<string> keys)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);

            RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                  () =>
                                  {
                                      IRedisClient _client = GetRedisClient(name);
                                      try
                                      {
                                          tm.Start();
                                          _client.RemoveAll(keys);
                                      }
                                      finally
                                      {
                                          ReleaseRedisClient(name, _client);
                                          tm.Stop();
                                          MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                      }
                                  });

        }

        public bool RemoveFromHash<TKey, TValue>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                                     () =>
                                     {
                                         IRedisClient _client = GetRedisClient(name);
                                         try
                                         {
                                             var client = _client.GetTypedClient<TValue>();
                                             var hash = client.GetHash<TKey>(hashID);

                                             tm.Start();
                                             return client.RemoveEntryFromHash<TKey>(hash, key);
                                         }
                                         finally
                                         {
                                             ReleaseRedisClient(name, _client);
                                             tm.Stop();
                                             MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                         }
                                     });
        }

        public bool RemoveFromOrderedHash<TKey>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                                     () =>
                                     {
                                         IRedisClient _client = GetRedisClient(name);
                                         try
                                         {
                                             tm.Start();
                                             return _client.RemoveItemFromSortedSet(hashID, key.ToString());
                                         }
                                         finally
                                         {
                                             ReleaseRedisClient(name, _client);
                                             tm.Stop();
                                             MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                         }
                                     });
        }

        public void ClearAllKeys()
        {
            _pool.ClearAllKeys();
        }

        #endregion

        #region Sorted Sets

        public double GetFromOrderedHash<TKey>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                return RetryUtility.RetryActionOnException<SocketException, double>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 return _client.GetItemScoreInSortedSet(hashID, key.ToString());
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 Thread.CurrentThread.CurrentCulture = oldCulture;
                                             }
                                         });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return -1;
        }

        public List<TKey> GetFromOrderedHash<TKey>(string name, string hashID, int fromRank, int toRank, StringParserHandler<TKey> parser)
        {
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, List<TKey>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 List<TKey> result = new List<TKey>();

                                                 tm.Start();

                                                 List<string> data = _client.GetRangeFromSortedSetDesc(hashID, fromRank, toRank);
                                                 foreach (string item in data)
                                                 {
                                                     result.Add(parser(item));
                                                 }

                                                 return result;
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 Thread.CurrentThread.CurrentCulture = oldCulture;
                                             }
                                         });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID + "/GetFromOrderedHash", e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/GetFromOrderedHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/GetFromOrderedHash", e);
            }
            return new List<TKey>();
        }

        public int IntersectOrderedHash<TKey>(string name, string referenceHashID, string restrictionHashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 return _client.StoreIntersectFromSortedSets(restrictionHashID, referenceHashID, restrictionHashID);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis - IntersectOrderedHash", referenceHashID, e);
            }
            return -1;
        }

        public int IntersectOrderedHash<TKey>(string name, string referenceHashID, string restrictionHashID, string resultHashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 return _client.StoreIntersectFromSortedSets(resultHashID, referenceHashID, restrictionHashID);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis - IntersectOrderedHash", referenceHashID, e);
            }
            return -1;
        }

        public int GetRankFromOrderedSet<TKey>(string name, string hashID, TKey key)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 return _client.GetItemIndexInSortedSetDesc(hashID, key.ToString());
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (KeyNotFoundException e)
            {
                ErrorHandling.ThrowKeyDoesNotExistException("Redis", hashID + "/GetRankFromOrderedSet", e);
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/GetRankFromOrderedSet", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/GetRankFromOrderedSet", e);
            }
            return -1;
        }

        public void SetRangeInOrderedHash<TKey>(string name, string hashID, IEnumerable<KeyValuePair<TKey, double>> range)
        {
            if (range.Count() > 0)
            {
                CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                try
                {
                    RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                             () =>
                                             {
                                                 IRedisClient _client = GetRedisClient(name);
                                                 try
                                                 {
                                                     foreach (KeyValuePair<TKey, double> item in range)
                                                     {
                                                         IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                                                         tm.Start();

                                                         _client.AddItemToSortedSet(hashID, item.Key.ToString(), item.Value);

                                                         tm.Stop();
                                                         MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                     }
                                                 }
                                                 finally
                                                 {
                                                     ReleaseRedisClient(name, _client);
                                                     Thread.CurrentThread.CurrentCulture = oldCulture;
                                                 }
                                             });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "SetRangeInOrderedHash", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", hashID, e);
                }
            }
        }

        public void SetRangeInOrderedHash<TKey>(string name, string hashID, IEnumerable<TKey> keys, double score)
        {
            if (keys.Count() > 0)
            {
                CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                try
                {
                    RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                             () =>
                                             {
                                                 IRedisClient _client = GetRedisClient(name);
                                                 try
                                                 {
                                                     IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                                                     tm.Start();

                                                     List<string> strKeys = new List<string>();
                                                     foreach (TKey item in keys)
                                                     {
                                                         strKeys.Add(item.ToString());
                                                     }

                                                     _client.AddRangeToSortedSet(hashID, strKeys, score);

                                                     tm.Stop();
                                                     MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 }
                                                 finally
                                                 {
                                                     ReleaseRedisClient(name, _client);
                                                     Thread.CurrentThread.CurrentCulture = oldCulture;
                                                 }
                                             });
                }
                catch (TimeoutException e)
                {
                    ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "SetRangeInOrderedHash", e);
                }
                catch (Exception e)
                {
                    ErrorHandling.ThrowGeneralException("Redis", hashID, e);
                }
            }
        }

        public void DuplicateOrderedHash<TKey>(string name, string hashID, string resultHashID)
        {
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
                                                 tm.Start();

                                                 _client.StoreUnionFromSortedSets(resultHashID, hashID);

                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "DuplicateOrderedHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
        }

        public void SetOrderedHash<TKey>(string name, string hashID, TKey key, double score)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 _client.AddItemToSortedSet(hashID, key.ToString(), score);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 Thread.CurrentThread.CurrentCulture = oldCulture;
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
        }

        public double IncrementOrderedHash<TKey>(string name, string hashID, TKey key, double incrementBy)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                return RetryUtility.RetryActionOnException<SocketException, double>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();

                                                 return _client.IncrementItemInSortedSet(hashID, key.ToString(), incrementBy);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 Thread.CurrentThread.CurrentCulture = oldCulture;
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID + "/" + key.ToString(), e);
            }
            return -1;
        }

        public ICollection<TKey> GetKeysFromOrderedHash<TKey>(string name, string hashID, StringParserHandler<TKey> parser)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            CultureInfo oldCulture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, ICollection<TKey>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 List<TKey> result = new List<TKey>();
                                                 foreach (string key in _client.GetAllItemsFromSortedSetDesc(hashID))
                                                 {
                                                     result.Add(parser(key));
                                                 }
                                                 return result;
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                                 Thread.CurrentThread.CurrentCulture = oldCulture;
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetKeysFromHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return null;
        }

        public ICollection<double> GetValuesFromOrderedHash<TKey>(string name, string hashID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, ICollection<double>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 return _client.GetAllWithScoresFromSortedSet(hashID).Values;
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", hashID + "/" + "GetValuesFromOrderedHash", e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", hashID, e);
            }
            return null;
        }

        #endregion

        #region Sets

        public void AddInSet<TValue>(string name, string setID, TValue value)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 _client.AddItemToSet(setID, value.ToString());
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID + "/" + value.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID + "/" + value.ToString(), e);
            }
        }

        public void AddRangeInSet<TValue>(string name, string setID, IEnumerable<TValue> values)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 List<string> items = new List<string>();
                                                 foreach (TValue value in values)
                                                 {
                                                     items.Add(value.ToString());
                                                 }
                                                 _client.AddRangeToSet(setID, items);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID, e);
            }
        }

        public bool SetContainsKey<TValue>(string name, string setID, TValue value)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, bool>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 return _client.SetContainsItem(setID, value.ToString());
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID + "/" + value.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID + "/" + value.ToString(), e);
            }
            return false;
        }

        public void RemoveInSet<TValue>(string name, string setID, TValue value)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 _client.RemoveItemFromSet(setID, value.ToString());
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID + "/" + value.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID + "/" + value.ToString(), e);
            }
        }

        /// <summary>
        /// TODO: change this method since it makes 1 call for each random values to get in redis
        /// </summary>
        public HashSet<string> GetRandomKeysFromSet(string name, string setID, int nbValues)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, HashSet<string>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 HashSet<string> result = new HashSet<string>();
                                                 if (_client.GetSetCount(setID) < nbValues)
                                                 {
                                                     return _client.GetAllItemsFromSet(setID);
                                                 }
                                                 
                                                 for (int i = 0; i < nbValues; i++)
                                                 {
                                                     int c = 0;
                                                     string value = string.Empty;
                                                     do
                                                     {
                                                         c++;
                                                         value = _client.GetRandomItemFromSet(setID);
                                                     } while (result.Contains(value) && c < 10);
                                                     
                                                     if (!result.Contains(value))
                                                     {
                                                        result.Add(value);
                                                     }
                                                 }
                                                 return result;
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID, e);
            }
            return new HashSet<string>();
        }

        public List<string> GetAllFromSet(string name, string setID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, List<string>>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 return new List<string>(_client.GetAllItemsFromSet(setID));
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID, e);
            }
            return new List<string>();
        }

        public int GetSetCount(string name, string setID)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                return RetryUtility.RetryActionOnException<SocketException, int>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 return _client.GetSetCount(setID);
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", setID, e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", setID, e);
            }
            return -1;
        }

        #endregion

        #region Expiration

        public void ExpireEntryIn<TKey>(string name, TKey key, int expiresInMinute)
        {
            IStopwatch tm = MonitoringTimers.Current.GetNewStopwatch(false);
            try
            {
                RetryUtility.RetryActionOnException<SocketException>(NUM_RETRIES, RETRY_TIMEOUT,
                                         () =>
                                         {
                                             IRedisClient _client = GetRedisClient(name);
                                             try
                                             {
                                                 tm.Start();
                                                 _client.ExpireEntryIn(key.ToString(), TimeSpan.FromMinutes(expiresInMinute));
                                             }
                                             finally
                                             {
                                                 ReleaseRedisClient(name, _client);
                                                 tm.Stop();
                                                 MonitoringTimers.Current.AddTime(Counters.Redis_Requests, tm);
                                             }
                                         });
            }
            catch (TimeoutException e)
            {
                ErrorHandling.ThrowTimeoutException("Redis", key.ToString(), e);
            }
            catch (Exception e)
            {
                ErrorHandling.ThrowGeneralException("Redis", key.ToString(), e);
            }
        }

        #endregion

        #region Utils

        private RedisClientPool _pool = new RedisClientPool();
        private IRedisClient GetRedisClient(string name)
        {
            return _pool.GetClient(name);
        }

        private void ReleaseRedisClient(string name, IRedisClient client)
        {
            if (client != null)
            {
                _pool.ReleaseClient(name, client);
            }
        }

        #endregion
    }
}
