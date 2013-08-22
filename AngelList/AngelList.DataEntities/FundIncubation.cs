using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class FundIncubation
	{
		[BsonId]
		public ObjectId Id { get; private set; }
		public DateTime? LastUpdate { get; set; }
		public Entity Fund { get; set; }
		public List<Incubation> Incubations { get; set; }

		public FundIncubation()
		{

		}

		public FundIncubation(FundIncubation original, Entity fund, List<Incubation> incubations)
		{
			Id = original.Id;
			Fund = fund;
			Incubations = incubations;
			LastUpdate = DateTime.UtcNow;
		}

		public override string ToString()
		{
			return string.Format("The fund {0} {1} invested in {2} startups the {3}", Fund.Id, Fund.Name, Incubations.Count, LastUpdate);
		}
	}
}
