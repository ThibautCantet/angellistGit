using Newtonsoft.Json;
using System.Collections.Generic;

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
	[JsonObject]
	public class GraphNode
	{
		[JsonProperty(PropertyName = "props")]
		public Dictionary<string, object> Properties = new Dictionary<string, object>();

		[JsonProperty(PropertyName = "keyName")]
		public string KeyName { get; set; }

		[JsonProperty(PropertyName = "rels")]
		public List<GraphRelation> Relations = new List<GraphRelation>();
	}
}
