using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Startup
{
	public class Startup
	{
		public int id { get; set; }
		public string name { get; set; }
		public string angellist_url { get; set; }
		public string product_desc { get; set; }
		public string high_concept { get; set; }
		public string company_url { get; set; }
	}
}
