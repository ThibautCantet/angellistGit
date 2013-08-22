// <copyright file="Item.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

using System.Collections.Specialized;

namespace AntVoice.Common.DataAccess.NeoClient.Handler.Data
{
    using System;
    using Tools.Extensions;
    using Attribute;

    [Serializable]
    public class Product : BaseNode
	{
		[Graphable]
        public int ProductId { get; set; }

		[Graphable]
        public string Label { get; set; }

		[Graphable]
        public DateTimeOffset? CreationDate { get; set; }

        [Graphable]
        public int WebSiteId { get; set; }

        [Graphable]
        public int ItemStatus { get; set; }

        [Graphable]
        public bool IsNew { get; set; }

        [Graphable]
        public bool IsPromotion { get; set; }

        [Graphable]
        public bool MustBeHighlighted { get; set; }

        public DateTimeOffset? LastUpdate { get; set; }

        /// <summary>
        /// Unique client product id
        /// </summary>
        [Graphable]
        public string ClientProductId { get; set; }

        /// <summary>
        /// Conversion nécessaire pour neo4j
        /// </summary>
        [Graphable(FieldName = "LastUpdate")]
        public int LastUpdateTimestamp
        {
            get
            {
                return LastUpdate.HasValue ? LastUpdate.Value.ToTimestamp() : 0;
            }
            set
            {
                LastUpdate = value.FromTimestampToDateTime();
            }
        }

        [Graphable]
        public StringDictionary ExtraProperties { get; set; }


        public Product(int id, DateTimeOffset? creationDate, string label, int webSiteId, int status, bool isNew, bool isPromotion, bool mustBeHighlighted, DateTimeOffset? lastUpdate, StringDictionary extraProperties, string clientProductId)
        {
            this.ProductId = id;
            this.CreationDate = creationDate;
            this.Label = label;
            this.ItemStatus = status;
            this.IsNew = isNew;
            this.WebSiteId = webSiteId;
            this.IsPromotion = isPromotion;
            this.MustBeHighlighted = mustBeHighlighted;
            this.LastUpdate = lastUpdate;
            this.ExtraProperties = extraProperties;
            this.ClientProductId = clientProductId;
        }

        public const string PRODUCT_INDEX_KEY = "ProductId";

        public override long GetBusinessKey()
        {
            return this.ProductId;
        }

        public override string GetBusinessKeyName()
        {
            return PRODUCT_INDEX_KEY;
        }

        public DateTimeOffset? GetCreationDate()
        {
            return CreationDate;
        }
    }
}
