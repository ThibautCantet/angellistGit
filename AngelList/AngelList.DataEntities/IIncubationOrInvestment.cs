using System;
namespace AngelList.DataEntities
{
	public interface IIncubationOrInvestment
	{
		string DateActivityFeed { get; set; }
		string Description { get; set; }
		int FundId { get; set; }
		string Id { get; set; }
		Entity Startup { get; set; }
	}
}
