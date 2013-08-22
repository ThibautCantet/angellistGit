using Platform.Tools.Logging;
using Platform.Tools.Pools.Abstract;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Platform.Tools.Pools
{
    /// <summary>
    /// This class provides methods to manage a read (slave)/write (master) pool.
    /// It's possible to add as many clients as desired in the pool.
    /// 
    /// It aims to manage failovers scenario when a master or a slave is not up anymore
    /// </summary>
    public class ReadWritePool
    {
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private static Dictionary<Pools, Dictionary<IPoolable, bool>> _writeHosts = new Dictionary<Pools, Dictionary<IPoolable, bool>>();
        private static Dictionary<Pools, Dictionary<IPoolable, bool>> _readHosts = new Dictionary<Pools, Dictionary<IPoolable, bool>>();

        private static ConcurrentDictionary<IPoolable, HostStatus> _checkIfAvailable = new ConcurrentDictionary<IPoolable, HostStatus>();

        static ReadWritePool()
		{
			Thread thread = new Thread(() =>
			{
				while (true)
				{
                    try
                    {
                        foreach (IPoolable host in _checkIfAvailable.Keys)
                        {
                            HostStatus status = _checkIfAvailable[host];
                            if (status.HasToCheck() && status.CheckAvailability())
                            {
                                _lock.EnterWriteLock();
                                try
                                {
                                    Dictionary<IPoolable, bool> availability = status.IsWriter ? _writeHosts[status.Pool] : _readHosts[status.Pool];
                                    availability[host] = true;

                                    _checkIfAvailable.TryRemove(host, out status);
                                }
                                finally
                                {
                                    _lock.ExitWriteLock();
                                }
                            }
                        }
                    }
                    finally
                    {
                        Thread.Sleep(2000);
                    }
				}
			});
			thread.Name = "ReadWritePool Host Status";
			thread.IsBackground = true;
			thread.Start();
		}

        #region Public methods

        public static bool IsPoolReady(Pools pool)
        {
            _lock.EnterReadLock();
            try
            {
                return _writeHosts.ContainsKey(pool) && _readHosts.ContainsKey(pool);
            }
            catch (Exception e)
            {
                Logger.Current.Error("ReadWritePool.IsPoolReady", "Error in ReadWritePool", e);
                return false;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static void AddReadHosts(Pools pool, IEnumerable<IPoolable> hosts)
        {
            AddHosts(pool, _readHosts, hosts);
        }

        public static void AddWriteHosts(Pools pool, IEnumerable<IPoolable> hosts)
        {
            AddHosts(pool, _writeHosts, hosts);
        }

        /// <summary>
        /// Gets the next available read host. If no read host is available, a write host will be returned
        /// Otherwise, host will be null, and false will be returned
        /// </summary>
        public static bool TryGetReadHost(Pools pool, out IPoolable host)
        {
            host = GetHost(_readHosts[pool]);
            if (host == null)
                // No slave host available, trying to get a master host
            {
                host = GetHost(_writeHosts[pool]);
                if (host == null)
                    // No host available at all
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the next available write host. If no write host is available, a read host will be returned since it should have become the master of the pool
        /// Otherwise, host will be null, and false will be returned
        /// </summary>
        public static bool TryGetWriteHost(Pools pool, out IPoolable host)
        {
            host = GetHost(_writeHosts[pool]);
            if (host == null)
            // No slave host available, it means that slave has become the master if it exists
            {
                host = GetHost(_readHosts[pool]);
                if (host == null)
                // No host available at all
                {
                    return false;
                }
            }
            return true;
        }

        public static void SetHostUnavailable(Pools pool, IPoolable host)
        {
            if (host != null)
            {
                _lock.EnterWriteLock();
                try
                {
                    if (_readHosts.ContainsKey(pool) && _readHosts[pool].ContainsKey(host))
                    {
                        _readHosts[pool][host] = false;
                        _checkIfAvailable.TryAdd(host, new HostStatus(pool, false,  host));
                    }

                    if (_writeHosts.ContainsKey(pool) && _writeHosts[pool].ContainsKey(host))
                    {
                        _writeHosts[pool][host] = false;
                        _checkIfAvailable.TryAdd(host, new HostStatus(pool, true, host));
                    }
                }
                catch (Exception e)
                {
                    Logger.Current.Error("ReadWritePool.SetHostUnavailable", "Error in ReadWritePool", e, pool, host);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }

        #endregion

        private static IPoolable GetHost(Dictionary<IPoolable, bool> availability)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var pair in availability)
                {
                    if (pair.Value)
                        return pair.Key;
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("ReadWritePool.GetHost", "Error in ReadWritePool", e);
            }
            finally
            {
                _lock.ExitReadLock();
            }
            return null;
        }

        private static void AddHosts(Pools pool, Dictionary<Pools, Dictionary<IPoolable, bool>> availability, IEnumerable<IPoolable> hostsToAdd)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!availability.ContainsKey(pool))
                    availability[pool] = new Dictionary<IPoolable, bool>();

                foreach (IPoolable host in hostsToAdd)
                {
                    if (!availability[pool].ContainsKey(host))
                    {
                        availability[pool][host] = true;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("ReadWritePool.AddHosts", "Error in ReadWritePool", e, pool);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
