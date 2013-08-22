using Platform.Tools.Monitoring.Interfaces;
using Platform.Tools.Monitoring.TimeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring
{
    public class MonitoringTimers
    {
        #region Singleton

        private readonly static MonitoringTimers _instance = new MonitoringTimers();

        private MonitoringTimers() { Initialize(); }

        public static MonitoringTimers Current { get { return _instance; } }

        public void Initialize()
        {
            ZabbixConfig.Current.Initialize();
        }

        #endregion

        public void AddTime(Counters counterName, IStopwatch watch)
        {
            TimeMonitorRepository.SaveTime(counterName.ToString(), watch.ElapsedNanoseconds);
        }

        public void AddError(Counters counterName)
        {
            TimeMonitorRepository.SaveTime(counterName.ToString(), 0L);
        }

        /// <summary>
        /// Gets a new Stopwatch to monitor a call method. Be sure to call MonitoringTimers.Current.AddTime after to save that time !
        /// </summary>
        /// <param name="start">Is the Stopwatch started or not ?</param>
        public IStopwatch GetNewStopwatch(bool start)
        {
            IStopwatch result = new TimeMeasurement();

            if (start)
            {
                result.Start();
            }

            return result;
        }
    }
}
