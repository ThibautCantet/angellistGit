using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring.Protocol
{
    internal interface IProcessor
	{
		List<TrapperItem> GenerateItems();
	}
}
