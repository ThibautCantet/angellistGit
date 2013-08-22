// <copyright file="Graphable.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Attribute
{
	using System;

	/// <summary>
	/// Used to tell which field should be included in the graph node
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class Graphable : Attribute
	{
		public bool AsJson { get; set; }

		public string FieldName { get; set; }
	}
}
