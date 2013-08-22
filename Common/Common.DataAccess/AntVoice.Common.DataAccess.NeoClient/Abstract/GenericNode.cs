// <copyright file="GenericNode.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Abstract
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Neo4jClient;
    using AntVoice.Common.Tools.Extensions;

    /// <summary>
    /// Represents generic node to populate graph database
    /// </summary>
    public class GenericNode : ISerializable
    {
        public const string LABEL = "Label";

        public string Label { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public GenericNode() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericNode" /> class when deserializing it from stream.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        public GenericNode(SerializationInfo info, StreamingContext context)
        {
            foreach (var entry in info)
            {
                if (entry.Name == LABEL)
                    Label = info.GetString(LABEL);
                else
                {
                    if (Metadata == null) Metadata = new Dictionary<string, object>();

                    Metadata.Add(entry.Name, entry.Value);
                }
            }
        }

        /// <summary>
        /// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(LABEL, Label);
            if (Metadata != null)
            {
                foreach (var data in Metadata)
                    info.AddValue(data.Key, data.Value ?? string.Empty);
            }

        }
    }

}