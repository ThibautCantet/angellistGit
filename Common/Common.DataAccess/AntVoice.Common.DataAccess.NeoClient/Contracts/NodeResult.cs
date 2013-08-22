// <copyright file="NodeResult.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Contracts
{
    using Neo4jClient;

    public class NodeResult<TSource>
    {
        public readonly NodeReference<TSource> Reference;
        public readonly TSource Node;

        public NodeResult(NodeReference<TSource> reference, TSource node)
        {
            Reference = reference;
            Node = node;
        }
    }
}