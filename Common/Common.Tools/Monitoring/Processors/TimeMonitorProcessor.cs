using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using Platform.Tools.Monitoring.Protocol;
using Platform.Tools.Monitoring.TimeManagement;
using Platform.Tools.Monitoring.TrapperItems;

namespace Platform.Tools.Monitoring.Processors
{
    internal class TimeMonitorProcessor : IProcessor
	{
		private string FormatDouble(double number)
		{
			return number.ToString("F10", CultureInfo.InvariantCulture);
		}

		public List<TrapperItem> GenerateItems()
		{
			return GetTimeMonitors();
		}

		private List<TrapperItem> GetTimeMonitors()
		{
			List<TrapperItem> result = new List<TrapperItem>();

			List<string> names = TimeMonitorRepository.GetTimeMonitorNames();
			foreach (string name in names)
			{
				TimeMonitor tm = TimeMonitorRepository.GetTimeMonitor(name);

				double totalCall1 = tm.GetTotalCall1();

				double totalTime1 = tm.GetTotalTime1();

				result.Add(new TimeMonitorTrapperItem(name + "_call1", FormatDouble(totalCall1)));

				result.Add(new TimeMonitorTrapperItem(name + "_time1", FormatDouble(totalTime1)));

				result.Add(new TimeMonitorTrapperItem(name + "_timeCall1", FormatDouble(0D == totalCall1 ? 0D : totalTime1 / totalCall1)));
			}

			return result;
		}
	}
}
