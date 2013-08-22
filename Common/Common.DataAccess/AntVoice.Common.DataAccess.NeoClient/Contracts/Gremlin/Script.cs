// <copyright file="Batch.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Contracts.Gremlin
{
	using System;
	using Newtonsoft.Json;
	using System.Collections.Generic;

	[JsonObject]
	public class Script
	{
		[JsonProperty("script")]
		public string ScriptStr { get; set; }

		[JsonProperty("params")]
		public Dictionary<string, object> ParameterMap { get; set; }

		public Script()
		{
			ParameterMap = new Dictionary<string, object>();
		}
	}
}
