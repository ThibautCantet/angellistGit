using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public static class Extensions
	{
		public static DateTime ToDateTime(this string value)
		{
			DateTime d;
			if (DateTime.TryParse(value, out d))
			{
				return d;
			}
			else
			{
				return DateTime.MinValue;
			}
		}
	}
}
