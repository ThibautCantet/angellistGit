using Platform.Tools.Pools.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Pools
{
    internal class HostStatus
    {
        public Pools Pool { get; set; }
        public bool IsWriter { get; set; }

        private IPoolable _poolable;
        private DateTime _lastCheck = DateTime.UtcNow;
        private int _timeChecked = 0;

        public HostStatus(Pools pool, bool isWriter, IPoolable poolable)
        {
            Pool = pool;
            IsWriter = isWriter;
            _poolable = poolable;
        }

        public bool HasToCheck()
        {
            if (_timeChecked == 0) return true;
            
            DateTime now = DateTime.UtcNow;
            int seconds = 2 ^ _timeChecked;
            if ((now - _lastCheck).TotalSeconds >= seconds)
            {
                return true;
            }
            return false;
        }

        public bool CheckAvailability()
        {
            _timeChecked++;
            bool result = _poolable.IsWorking();
            if (!result)
            {
                _lastCheck = DateTime.UtcNow;
                _timeChecked++;
            }
            return result;
        }
    }
}
