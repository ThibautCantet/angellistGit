using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class Entity
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Url { get; set; }
		public string TagLine { get; set; }

		public override string ToString()
		{
			return string.Format("{0} {1}", Id, Name);
		}
	}
}
