using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring.TimeManagement
{
    internal class Counter
	{
		public long Call { get; set; }
		public long Time { get; set; }

		public Counter()
		{
			Call = 0;
			Time = 0;
		}

		public void AddTime(long elapsed)
		{
			++Call;
			Time += elapsed;
		}
	}
}
