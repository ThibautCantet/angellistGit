// <copyright file="GraphRelation.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>


namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
    using Newtonsoft.Json;

    [JsonObject]
    public class GraphRelation
    {
        [JsonProperty("label")]
        public string Label { get; set; }

		[JsonProperty("creationDate")]
        public int CreationDate { get; set; }

        [JsonProperty("target")]
        public GraphNode Target { get; set; }

        public GraphRelation()
        {
        }

        public override string ToString()
        {
			return string.Format("to {0}:{1} data : {2}", Target.KeyName, Target.Properties[Target.KeyName], Label);
        }
    }
}