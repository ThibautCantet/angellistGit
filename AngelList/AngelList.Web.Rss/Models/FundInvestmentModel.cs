using AngelList.DataEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;

namespace AngelList.Web.Rss.Models
{
	public class FundInvestmentModel : SyndicationItem
	{
		public FundInvestmentModel(FundInvestment fundInvestement)
		{
			//Id = fundInvestement.Id;
		}
	}
}