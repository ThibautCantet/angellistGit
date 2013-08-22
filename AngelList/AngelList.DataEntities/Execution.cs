using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataEntities
{
	public class Execution
	{
		[BsonId]
		public int Id { get; set; }
		public DateTime LastUpdateInvestment { get; set; }
		public DateTime LastUpdateIncubation { get; set; }
		public DateTime LastDateCrmSent { get; set; }
		public DateTime LastDateRss { get; set; }
	}
}
