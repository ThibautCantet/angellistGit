using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public abstract class FundAction
	{
		[BsonId]
		public ObjectId Id { get; private set; }
		public DateTime? LastUpdate { get; set; }
		public Entity Fund { get; set; }
		public List<IIncubationOrInvestment> IncubationOrInvestment { get; set; }

		public FundAction()
		{

		}

		public FundAction(FundAction original, Entity fund, List<IIncubationOrInvestment> incubations)
		{
			Id = original.Id;
			Fund = fund;
			IncubationOrInvestment = incubations;
			LastUpdate = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return string.Format("The fund {0} {1} invested in {2} startups the {3}", Fund.Id, Fund.Name, IncubationOrInvestment.Count, LastUpdate);
		}
	}
}
