using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public abstract class IncubationOrInvestment
	{
		public string Id { get; set; }
		public int FundId { get; set; }
		public Entity Startup { get; set; }
		public string Description { get; set; }
		public string DateActivityFeed { get; set; }
	}
}
