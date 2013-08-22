using AngelList.Business;
using AngelList.Business.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList
{
	class Program
	{
		static void Main(string[] args)
		{
			TimeSpan t;
			bool success = TimeSpan.TryParse("2013-08-21T01:09:48Z", out t);

			DateTime d;

			success = DateTime.TryParse("2013-08-21T01:09:48Z", out d);
			//var res = StartupStream.GetStartups();
			var dic = StartupRoleStream.GetStartupRoles();
			InvestmentStream.GetHistory();
		//	InvestmentStream.CorrectFundInvestments();
		//	CrmReport.Send();

			InvestmentStream.GetInvestmentsFromFeed();
			IncubationStream.GetIncubationsFromFeed();
			//Console.Write(res);
		}
	}
}
