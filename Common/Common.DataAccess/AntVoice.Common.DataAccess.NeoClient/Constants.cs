// <copyright file="Constants.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient
{
    public static class Constants
    {
        public const string ToNode = "/node";
		public const string ToProperties = "/properties";

		/// <summary>
		/// Used to modified relationship property
		/// </summary>
		public const string ToRelationShip = "/relationship";

		/// <summary>
		/// Used to create a new relationship between nodes
		/// </summary>
		public const string ToRelationShips = "/relationships";

		public const string ToIndex = "/index/node";
		public const string ToCypher = "/cypher";
        public const string HttpGet = "GET";
        public const string HttpPost = "POST";
        public const string HttpPut = "PUT";
    }
}