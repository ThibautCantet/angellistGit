using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring.TimeManagement
{
    internal static class TimeFunctionHelper
	{
		public static long GetCurrentMillisecond(this DateTime date)
		{
			return date.Ticks / 10000L;
		}

		public static long GetCurrentNanosecond(this DateTime date)
		{
			return date.Ticks * 100L;
		}
	}
}
