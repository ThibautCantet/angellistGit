using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
	/*
	 * [{"id":1,"body":{
		  "columns" : [ "r" ],
		  "data" : [ ]
		},"from":"/cypher"}]
	 */

	[JsonObject]
	public class GraphQueryResultBody
	{
		[JsonProperty("columns")]
		public string[] Columns { get; set; }

		[JsonProperty("data")]
		public object[][] Data { get; set; }
	}

	[JsonObject]
	public class GraphQueryResult
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("body")]
		public GraphQueryResultBody Body { get; set; }

		[JsonProperty("from")]
		public string From { get; set; }

	}
}
