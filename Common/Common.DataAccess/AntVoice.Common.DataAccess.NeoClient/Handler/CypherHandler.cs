// <copyright file="CypherHandler.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler
{
    using AntVoice.Common.DataAccess.NeoClient.Abstract;
    using AntVoice.Common.DataAccess.NeoClient.Contracts;
    using AntVoice.Common.DataAccess.NeoClient.Handler.Data;
    using System;
    using System.Collections.Generic;

	public class CypherHandler : IGraphHandle
	{
		public List<GraphAction> _actions { get; set; }

		private Neo4jContext _context;

		public const string CreateIfNotExists = "START n=node:node_auto_index('{0}:{1}') WITH count(*) as c WHERE c=0 CREATE x=(node{{User}})";
		private const string CreateRelIfDestExists = "START n=node:node_auto_index('{0}:{1}'), m=node:node_auto_index('{2}:{3}') CREATE UNIQUE n-[:{4}{{RelProp}}]-m";
		private const string CreateDirectionalRelIfDestExists = "START n=node:node_auto_index('{0}:{1}'), m=node:node_auto_index('{2}:{3}') CREATE UNIQUE n-[:{4}{{RelProp}}]->m";

        private BaseNode _node;
		private IEnumerable<Relationship> _rels;

		public CypherHandler(BaseNode node, IEnumerable<Relationship> rels)
		{
			_actions = new List<GraphAction>();
			_context = new Neo4jContext(true);

			_node = node;
			_rels = rels;
		}

		public bool Handle()
		{
			CreateNodeAndRelationships(_node, _rels);
			return true;
		}

        private void CreateNodeAndRelationships(BaseNode node, IEnumerable<Relationship> rels)
		{
			foreach (var r in rels)
			{
				string query = r.IsDirectional ? CreateDirectionalRelIfDestExists : CreateRelIfDestExists;
				_actions.Add(GraphAction.CreateCypherAction(new GraphQuery
				{
					Query = string.Format(query, node.GetBusinessKeyName(), node.GetBusinessKey(), r.Target.GetBusinessKeyName(), r.Target.GetBusinessKey(), r.GetRelationType()),
					Params = new Dictionary<string, object>() { { "RelProp", r.GetRelationshipDetail() } }
				}));
			}

			Query();
		}

		private void Query()
		{
			_context.InsertBatch(_actions);
		}
	}
}
