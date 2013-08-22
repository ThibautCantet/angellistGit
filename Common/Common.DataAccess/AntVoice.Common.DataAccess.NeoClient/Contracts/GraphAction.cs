// <copyright file="GraphAction.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>


namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
    using Newtonsoft.Json;

    [JsonObject]
    public class GraphAction
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("method")]
        public string Method { get; set; }
        [JsonProperty("to")]
        public string To { get; set; }
        [JsonProperty("body")]
        public object Body { get; set; }

        public GraphAction()
        {
        }

        public GraphAction(int id, string method, string to, object body)
        {
            Id = id;
            Method = method;
            To = to;
            Body = body;
        }

		public static GraphAction CreateCypherAction(GraphQuery body)
		{
			return new GraphAction
			{
				Method = Constants.HttpPost,
				To = Constants.ToCypher,
				Id = 0,
				Body = body
			};
		}

        public override string ToString()
        {
            return string.Format("Id : {0} - Method : {1} - To : {2} - Body {3}", Id, Method, To, Body);
        }
    }
}