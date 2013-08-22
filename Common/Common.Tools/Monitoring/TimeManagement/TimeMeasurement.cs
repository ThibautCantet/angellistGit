using Platform.Tools.Monitoring.Interfaces;
using System;

namespace Platform.Tools.Monitoring.TimeManagement
{
	public class TimeMeasurement : IStopwatch
	{
		public long ElapsedNanoseconds
		{
			get
			{
				return (_end - _start) * 100L;
			}
		}

		private long _start = -1;
		private long _end = -1;

		public void Start()
		{
			_start = DateTime.Now.Ticks;
		}

		public void Stop()
		{
			_end = DateTime.Now.Ticks;
		}
	}
}
