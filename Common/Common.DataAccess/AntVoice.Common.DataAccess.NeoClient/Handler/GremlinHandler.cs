// <copyright file="GenericHandler.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler
{
    using AntVoice.Common.DataAccess.NeoClient.Abstract;
    using AntVoice.Common.DataAccess.NeoClient.Contracts;
    using AntVoice.Common.DataAccess.NeoClient.Contracts.Gremlin;
    using AntVoice.Common.DataAccess.NeoClient.Handler.Data;
    using AntVoice.Common.Tools.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Text;

	public class GremlinHandler : ScriptBuilder, IGraphHandle
	{
        protected BaseNode Node { get; set; }

		protected List<Relationship> Rels { get; set; }

        protected Neo4jContext _context;

        public GremlinHandler(BaseNode node, IEnumerable<Relationship> rels)
			: base()
		{
			Rels = new List<Relationship>();
            _context = new Neo4jContext(false);

			Node = node;
			Rels.AddRange(rels);
		}

		public IEnumerable<GraphNodeResult> Handle()
		{
			CreateNodeAndRelationships();
			return null;
		}

		private void CreateNodeAndRelationships()
		{
			_context.ExecuteGremlinScript(new Script
			{
				ScriptStr = ParseGremlinQuery()
			});
		}

		private string ParseGremlinQuery()
		{
			StartNewScript();
			AppendStatement("userNode = null;")
				.AppendFormatStatement("idxRes = idx.get('{0}','{1}');", Node.GetBusinessKeyName(), Node.GetBusinessKey())
				.AppendStatement("if((userNode = idxRes.getSingle()) == null){")
				.AppendStatement(CreateNode(Node, "userNode"))
				.AppendStatement("idxRes.close();");

			foreach (Relationship rel in Rels)
			{
				AppendStatement("otherNode = null;")
					.AppendFormatStatement("idxRes = idx.get('{0}','{1}');", rel.Target.GetBusinessKeyName(), rel.Target.GetBusinessKey())
					.AppendStatement("if((otherNode = idxRes.getSingle()) == null){")
					.AppendStatement(CreateNode(rel.Target, "otherNode"))
					.AppendStatement("};idxRes.close();")
					.AppendFormatStatement("rel = userNode.createRelationshipTo(otherNode, DynamicRelationshipType.withName('{0}'));", rel.GetRelationType())
					.AppendFormatStatement("rel.setProperty('Label', '{0}');", rel.Label)
                    .AppendFormatStatement("rel.setProperty('CreationDate', '{0}');", rel.CreationDate.ToTimestamp());
			}

			AppendStatement("};tx.success();tx.finish();");

			return GetScript();
		}

		private string CreateNode(BaseNode node, string nodeName)
		{
			var sb = new StringBuilder();
			var nodeProperty = "{0}.setProperty('{1}', {2});";
			var nodeStrProperty = "{0}.setProperty('{1}', '{2}');";
			var genNode = node.ToGraphNode();

			sb.AppendFormat("{0} = neo4j.createNode();", nodeName);
			sb.AppendFormat(nodeStrProperty, nodeName, "Label", genNode.Label);
			foreach (var k in genNode.Metadata.Keys)
			{
				var stmt = nodeProperty;
				if (genNode.Metadata[k] is String)
				{
					stmt = nodeStrProperty;
				}

				var value = genNode.Metadata[k];
				if (genNode.Metadata[k] is DateTimeOffset)
				{
                    value = ((DateTimeOffset)genNode.Metadata[k]).ToTimestamp();
				}
				else if (genNode.Metadata[k] is DateTime)
				{
                    value = new DateTimeOffset((DateTime)genNode.Metadata[k]).ToTimestamp();
				}
				else if (genNode.Metadata[k] is Enum)
				{
					value = (int)genNode.Metadata[k];
				}
				sb.AppendFormat(stmt, nodeName, k, value);
			}

			return sb.ToString();
		}
	}
}
