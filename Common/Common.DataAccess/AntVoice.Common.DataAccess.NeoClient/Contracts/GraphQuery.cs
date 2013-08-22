// <copyright file="GraphQuery.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
    using System;
    using Newtonsoft.Json;
    using System.Collections.Generic;

    [JsonObject]
    public class GraphQuery
    {
        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, object> Params { get; set; }

        public static GraphQuery CreateStaticQuery(string query)
        {
            return new GraphQuery
            {
                Query = query,
                Params = new Dictionary<string, object>()
            };
        }
    }
}
