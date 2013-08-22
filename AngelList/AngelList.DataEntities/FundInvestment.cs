using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class FundInvestment
	{
		[BsonId]
		public ObjectId Id { get; private set; }
		public DateTime? LastUpdate { get; set; }
		public Entity Fund { get; set; }
		public List<Investment> Investments { get; set; }

		public FundInvestment()
		{

		}

		public FundInvestment(FundInvestment original, Entity fund, List<Investment> investments)
		{
			Id = original.Id;
			Fund = fund;
			Investments = investments;
			LastUpdate = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return string.Format("The fund {0} {1} invested in {2} startups the {3}", Fund.Id, Fund.Name, Investments.Count, LastUpdate);
		}
	}
}
