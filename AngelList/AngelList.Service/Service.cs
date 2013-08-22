using AngelList.Business;
using AngelList.Business.CRM;
using Atlas;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AngelList.Service
{
	class Service : IAmAHostedProcess
    {
		private System.Timers.Timer _timerTasks;
		private System.Timers.Timer _timerTasksCrm;
        private ServiceConfig _config = new ServiceConfig();

		private bool _isRunning = false;
		private bool _isRunningCrm = false;

        public void Resume()
        {
            Console.WriteLine("Resuming...");
        }

        public void Pause()
        {
            Console.WriteLine("Pausing...");
        }

        public void Start()
        {
            _timerTasks = new Timer();
            // every 5 mintues
            _timerTasks.Interval = 5 * 60000;
            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)_timerTasks.Interval);
			Log.Info("AngelList_feed_processing", "refresh frequence : " + time + " ms");
            _timerTasks.Elapsed += new ElapsedEventHandler(this.TimerAngelList_Elapsed);
            _timerTasks.Enabled = true;

			TimerAngelList_Elapsed(null, null);

			_timerTasksCrm= new Timer();
            // every 24H
            _timerTasksCrm.Interval = 24 * 60 * 60000;
			_timerTasksCrm.Elapsed += new ElapsedEventHandler(this.TimerAngelListSendReport_Elapsed);
			_timerTasksCrm.Enabled = true;

			TimerAngelListSendReport_Elapsed(null, null);
        }

        private void TimerAngelList_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                try
                {
                    Log.Info("AngelList_feed", "update mongo with last investments feeds");

					InvestmentStream.GetInvestmentsFromFeed();

					Log.Info("AngelList_feed", "update mongo with last incubations feeds");

					IncubationStream.GetIncubationsFromFeed();
                }
                catch (Exception ex)
                {
                    LogEventLog(ex.Message);
                }
                finally
                {
                    _isRunning = false;
                }
            }
            else
            {
                Log.Info("ProcessFiles", "Tried to start a new processing, but a process is still in progress");
            }
        }

		private void TimerAngelListSendReport_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isRunningCrm)
            {
				_isRunningCrm = true;
                try
                {
                    Logger.Current.Info("AngelList_feed_send_email", "Beginning generation of the report");

					CrmReport.Send();
                }
                catch (Exception ex)
                {
                    LogEventLog(ex.Message);
                }
                finally
                {
					_isRunningCrm = false;
                }
            }
            else
            {
                Log.Info("ProcessFiles", "Tried to start a new processing, but a process is still in progress");
            }
        }

        public void Stop()
        {
            _timerTasks.Stop();
        }

		private void LogEventLog(string sEvent, EventLogEntryType level = EventLogEntryType.Error, string callingFunction = "TimerAngelList_Elapsed")
        {
            string sSource = "AngelList.Service";
            string sLog = "Application";

            if (!EventLog.SourceExists(sSource))
            {
                EventLog.CreateEventSource(sSource, sLog);
            }
            EventLog.WriteEntry(sSource, sEvent, level);

            switch (level)
            {
                case EventLogEntryType.Information:
                case EventLogEntryType.SuccessAudit:
                    Logger.Current.Info(callingFunction, "Info", sEvent);
                    break;
                case EventLogEntryType.Warning:
                    Logger.Current.Warn(callingFunction, "Warning", sEvent);
                    break;
                case EventLogEntryType.Error:
                case EventLogEntryType.FailureAudit:
                default:
                    Logger.Current.Error(callingFunction, "Error", sEvent);
                    break;
            }
        }
	}
}
