using AngelList.DataAccess;
using AngelList.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class ExecutionUpdater
	{
		#region call to the api
		internal static void InsertFirst()
		{
		//	ExecutionDataAccess.InsertFirst(new Execution() { Id = 1, LastUpdate = DateTime.UtcNow });
		}

		#region Investment
		internal static void UpdateInvestment()
		{
			ExecutionDataAccess.UpdateInvestment();
		}

		internal static DateTime GetLastUpdateInvestment()
		{
			return ExecutionDataAccess.GetLastUpdateInvestment();
		}
		#endregion
		#region Incubation
		internal static void UpdateIncubation()
		{
			ExecutionDataAccess.UpdateIncubation();
		}

		internal static DateTime GetLastUpdateIncubation()
		{
			return ExecutionDataAccess.GetLastUpdateIncubation();
		}
		#endregion
		#endregion

		#region Crm
		internal static void UpdateCrmSent()
		{
			ExecutionDataAccess.UpdateCrmSent();
		}

		internal static DateTime GetLastDateCrmSent()
		{
			return ExecutionDataAccess.GetLastDateCrmSent();
		}
		#endregion

		#region Rss
		public static void UpdateRss()
		{
			ExecutionDataAccess.UpdateRss();
		}

		internal static DateTime GetLastDateRss()
		{
			return ExecutionDataAccess.GetLastDateRss();
		}
		#endregion
	}
}
