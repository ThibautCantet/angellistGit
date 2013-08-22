using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class IncubationComparer : IEqualityComparer<Incubation>
	{

		public bool Equals(Incubation b1, Incubation b2)
		{
			return b1.Id == b2.Id;
		}

		public int GetHashCode(Incubation bx)
		{
			return bx.Id.GetHashCode();
		}
	}
}
