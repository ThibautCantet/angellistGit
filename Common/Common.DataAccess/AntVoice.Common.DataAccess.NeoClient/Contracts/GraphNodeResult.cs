// <copyright file="GraphNodeResult.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

using System.Diagnostics;

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
    using System.Linq;
    using Neo4jClient;
    using Newtonsoft.Json;

    [JsonObject]
    public class GraphNodeResult
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("from")]
        public string From { get; set; }
        [JsonProperty("body")]
        public object Body { get; set; }
        [JsonProperty("data")]
        public object Data { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("self")]
        public string Self { get; set; }
        public NodeReference NodeReference
        {
            get
            {
                if (!string.IsNullOrEmpty(Self))
                    return long.Parse(Self.Split('/').Last());
                if (!string.IsNullOrEmpty(Location))
                    return long.Parse(Location.Split('/').Last());
                return null;
            }
        }

        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("exception")]
        public string Exception { get; set; }
        [JsonProperty("stacktrace")]
        public string[] StackTrace { get; set; }

        public override string ToString()
        {
            return string.Format("Id : {0} - From : {1} - Body : {2} - Data {3} - Location : {4} - Self : {5}", Id, From, Body, Data, Location, Self);
        }
    }
}