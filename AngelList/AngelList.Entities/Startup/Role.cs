using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Entities.Startup
{
	public class Role
	{
		public int id { get; set; }
		public string role { get; set; }
		public Tagged tagged { get; set; }
		public Startup startup { get; set; }
	}
}
