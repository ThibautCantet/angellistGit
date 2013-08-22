using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Feed
{
	public abstract class ActiveEntity
	{
		public string type { get; set; }
		public int id { get; set; }
		public string name { get; set; }
		public string image { get; set; }
		public string angellist_url { get; set; }
		public int? system_user { get; set; }
		public string tagline { get; set; }
	}
}
