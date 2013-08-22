using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Startup
{
	public class Tagged
	{
		public int id { get; set; }
		public string role { get; set; }
		public string name { get; set; }
		public string bio { get; set; }
		public string angellist_url { get; set; }
		public string image { get; set; }
	}
}
