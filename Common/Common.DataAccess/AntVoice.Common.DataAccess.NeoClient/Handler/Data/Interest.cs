using AntVoice.Common.DataAccess.NeoClient.Handler.Data;
// <copyright file="Interest.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
	using System;
    using Attribute;

	[Serializable]
	public class Interest : BaseNode
	{
        /// <summary>
        /// The interest reference id
		/// </summary>
		[Graphable]
        public long InterestId { get; set; }

		/// <summary>
		/// The interest label
		/// </summary>
		[Graphable]
		public string Label { get; set; }

		/// <summary>
		/// Utiliser pour differencier les interest entre plateforme
		/// </summary>
		[Graphable]
		public string InterestType { get; set; }

        public Interest(long interestId, string interestType, string label)
		{
			this.InterestId = interestId;
			this.InterestType = interestType;
			this.Label = label;
		}

		public const string INTEREST_INDEX_KEY = "InterestId";

        public override long GetBusinessKey()
		{
			return InterestId;
		}

		public override string GetBusinessKeyName()
		{
			return INTEREST_INDEX_KEY;
		}
	}
}
