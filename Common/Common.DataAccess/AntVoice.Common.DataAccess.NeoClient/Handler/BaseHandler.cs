// <copyright file="BaseHandler.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient.Handler
{
	using AntVoice.Common.DataAccess.NeoClient.Abstract;
	using AntVoice.Common.DataAccess.NeoClient.Contracts;
	using AntVoice.Platform.Tools.Logging;
	using System.Collections.Generic;

	public abstract class BaseHandler : IGraphHandle
	{
		public List<GraphAction> Actions { get; set; }

        protected Neo4jContext _context;

		protected BaseHandler()
		{
			Actions = new List<GraphAction>();
            _context = new Neo4jContext(true);
		}

		protected void AddAction(GraphAction action)
		{
            if (action != null)
            {
                Actions.Add(action);
            }
		}

		protected void AddActions(IEnumerable<GraphAction> actions)
		{
			Actions.AddRange(actions);
		}

		protected IEnumerable<GraphAction> GetActions()
		{
			return Actions;
		}

		protected bool RunActions()
		{
			if (Actions.Count > 0)
			{
				_context.InsertBatch(Actions);
				return true;
			}
			else
			{
                Log.Debug("BaseHandler.RunActions", "No actions");
				return true;
			}
		}

		public abstract bool Handle();
	}
}
