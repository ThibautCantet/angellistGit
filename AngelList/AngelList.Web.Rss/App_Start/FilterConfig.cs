using System.Web;
using System.Web.Mvc;

namespace AngelList.Web.Rss
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}