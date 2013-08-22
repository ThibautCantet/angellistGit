using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
	public class InsertDataResult
	{
		[JsonProperty(PropertyName="status")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "message")]
		public string Message { get; set; }
	}
}
