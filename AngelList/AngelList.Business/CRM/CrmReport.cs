using AngelList.Crm;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business.CRM
{
	public class CrmReport
	{
		public static void Send()
		{
			try
			{
				var dicInvestments = InvestmentStream.GetNewFundInvestment();

				if (dicInvestments.Count > 0)
				{
					List<string> descriptions = new List<string>();
					foreach (var fund in dicInvestments)
					{
						foreach (var investment in fund.Value)
						{
							descriptions.Add(investment.Description);
						}
					}
					string info = string.Join("<br />", descriptions);

					DailyReport.Send(info);

					ExecutionUpdater.UpdateCrmSent();
					Log.Info("CrmReport.Send", "Updates sent by email");
				}
				else
				{
					Log.Info("CrmReport.Send", "No update");
				}
			}
			catch (Exception ex)
			{
				Log.Error("CrmReport.Send", "error", ex);
			}
		}
	}
}
