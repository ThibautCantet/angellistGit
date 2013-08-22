// <copyright file="Tag.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
    using Attribute;
    using System;

	[Serializable]
    public class Tag : BaseNode
	{
		[Graphable]
        public int TagId { get; set; }

		[Graphable]
		public string Label { get; set; }

		[Graphable]
        public int WebSiteId { get; set; }

		[Graphable]
		public string TagType { get; set; }

		public Tag(int id, string label, int webSiteId, string tagType)
		{
            this.TagId = id;
			this.Label = label;
            this.TagType = tagType;
            this.WebSiteId = webSiteId;
		}

		public const string TAG_INDEX_KEY = "TagId";

		public override long GetBusinessKey()
		{
		    return this.TagId;
		}

		public override string GetBusinessKeyName()
		{
			return TAG_INDEX_KEY;
		}
	}
}
