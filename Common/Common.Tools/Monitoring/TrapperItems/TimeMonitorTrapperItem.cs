using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Tools.Monitoring.Protocol;

namespace Platform.Tools.Monitoring.TrapperItems
{
	internal class TimeMonitorTrapperItem : TrapperItem
	{
		public TimeMonitorTrapperItem(string key, string value) : base(key, value) { }

		protected override string Prefix
		{
			get { return "ntm"; }
		}
	}
}
