using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class InvestmentComparer : IEqualityComparer<Investment>
	{

		public bool Equals(Investment b1, Investment b2)
		{
			return b1.Id == b2.Id;
		}

		public int GetHashCode(Investment bx)
		{
			return bx.Id.GetHashCode();
		}
	}
}
