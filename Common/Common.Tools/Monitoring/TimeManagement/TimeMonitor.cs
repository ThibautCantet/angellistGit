using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Platform.Tools.Monitoring.TimeManagement
{
    internal class TimeMonitor
	{
		private const long ROTATE_INTERVAL = 60L * 1000L;
		private const double ROTATE_INTERVAL_AS_DOUBLE = ROTATE_INTERVAL;

		private class TimeMonitorRotate
		{
			private long _lastRun = DateTime.Now.GetCurrentMillisecond();

			public void Do()
			{
				while (true)
				{
					long nextRun = _lastRun + ROTATE_INTERVAL;
					long sleepTime = nextRun - DateTime.Now.GetCurrentMillisecond();
					if (0 >= sleepTime)
					{
						sleepTime = ROTATE_INTERVAL;
						nextRun = DateTime.Now.GetCurrentMillisecond() + sleepTime;
					}

					Thread.Sleep(Convert.ToInt32(sleepTime));

					RotateMonitors();
					_lastRun = nextRun;
				}
			}

			private void RotateMonitors()
			{
				lock (_monitors)
				{
					foreach (TimeMonitor monitor in _monitors)
					{
						monitor.Rotate();
					}
				}
			}
		}

		private static LinkedList<TimeMonitor> _monitors = new LinkedList<TimeMonitor>();

		private List<Counter> _history = new List<Counter>(17);
		private Counter _total = new Counter();
		private Counter _current;

		private long _lastRotate = DateTime.Now.GetCurrentMillisecond();

		public string Name { get; set; }

		public TimeMonitor(string name)
		{
			Name = name;

			for (int i = 0; i < 16; i++)
			{
				_history.Add(new Counter());
			}
			_current = _history[0];

			lock (_monitors)
			{
				_monitors.AddLast(this);
			}
		}

		static TimeMonitor()
		{
			TimeMonitorRotate tmr = new TimeMonitorRotate();
			Thread thread = new Thread(tmr.Do);
			thread.Name = "TimeMonitor Rotate";
			thread.IsBackground = true;
			thread.Start();
		}

		public void AddTime(long elapsed)
		{
			_total.AddTime(elapsed);
			_current.AddTime(elapsed);
		}

		public long GetTotalCalls()
		{
			return _total.Call;
		}

		public long GetTotalTime()
		{
			return _total.Time;
		}

		private void Rotate()
		{
			Counter counter = new Counter();
			_current = counter;
			lock (_history)
			{
				_history.Insert(0, counter);
				while (16 < _history.Count)
				{
					_history.RemoveAt(16);
				}
			}
			_lastRotate = DateTime.Now.GetCurrentMillisecond();
		}

		public double GetTotalCall1()
		{
			return GetTotalCall(1);
		}

		public double GetTotalTime1()
		{
			return GetTotalTime(1);
		}

		private double GetTotalCall(int step)
		{
			long totalCall = 0;
			Counter a;
			Counter b;
			lock (_history)
			{
				a = _history[0];
				b = _history[step];

				for (int i = 1; i < step; ++i)
				{
					totalCall += _history[i].Call;
				}
			}
			long elapsed = DateTime.Now.GetCurrentMillisecond() - _lastRotate;
			return
					((double)(a.Call * elapsed + b.Call * (ROTATE_INTERVAL - elapsed)))
							/ ROTATE_INTERVAL_AS_DOUBLE
							+ ((double)totalCall);
		}

		private double GetTotalTime(int step)
		{
			long totalTime = 0;
			Counter a;
			Counter b;
			lock (_history)
			{
				a = _history[0];
				b = _history[step];

				for (int i = 1; i < step; ++i)
				{
					totalTime += _history[i].Time;
				}
			}
			long elapsed = DateTime.Now.GetCurrentMillisecond() - _lastRotate;
			return
					((double)(a.Time * elapsed + b.Time * (ROTATE_INTERVAL - elapsed)))
							/ ROTATE_INTERVAL_AS_DOUBLE
							+ ((double)totalTime);
		}
	}
}
