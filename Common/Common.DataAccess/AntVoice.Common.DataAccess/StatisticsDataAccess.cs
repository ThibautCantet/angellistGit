using AntVoice.Common.DataAccess.NeoClient;
using AntVoice.Common.DataAccess.NeoClient.Contracts.Gremlin;
using AntVoice.Common.Entities.Filters;
using AntVoice.Common.Entities.Neo;
using AntVoice.Common.Entities.Recommendation;
using AntVoice.Common.Entities.Settings.Recommendation;
using AntVoice.Platform.Tools.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess
{
	public class StatisticsDataAccess
	{
		public static CypherResult ExecuteMirrorQuery(GraphAction ga, AlgoContext context, int[] productIds)
		{
			if (ga.MirrorQuery != null)
			{
				return ExecuteMirrorCypherQuery(ga.MirrorQuery.QueryContent, context, productIds);
			}
			else if (ga.MirrorScript != null)
			{
				return ExecuteMirrorGremlin(ga.MirrorScript.MirrorScript, context, productIds);
			}

			return null;
		}

		private static CypherResult ExecuteMirrorGremlin(string script, AlgoContext context, int[] productIds)
		{
			try
			{
				Neo4jContext client = new Neo4jContext(false);
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				foreach (RecommendationContext item in context.Context.Keys)
				{
					parameters["pMap" + item.ToString()] = context.Context[item];
				}

				if (productIds != null && productIds.Count() > 0)
				{
					parameters["pMapProductsId"] = productIds;
				}

				string response = client.ExecuteGremlinScript(new Script { ScriptStr = script, ParameterMap = parameters });
				return JsonConvert.DeserializeObject<CypherResult>(response);
			}
			catch (Exception e)
			{
				Log.Error("ExecuteMirrorQuery", "Error executing query", e);
				throw;
			}
		}

		private static CypherResult ExecuteMirrorCypherQuery(string query, AlgoContext context, int[] productIds)
		{
			try
			{
				Log.Debug("ExecuteMirrorQuery", "Building query from parameters", query);
				string filteredQuery = BuildMirrorQuery(query, productIds);
				Log.Debug("ExecuteMirrorQuery", "Query built from parameters", filteredQuery);

				Neo4jContext client = new Neo4jContext(false);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				foreach (RecommendationContext item in context.Context.Keys)
				{
					parameters[item.ToString()] = context.Context[item];
				}
				string response = client.GetWithCypherQuery(filteredQuery, parameters);
				return JsonConvert.DeserializeObject<CypherResult>(response);
			}
			catch (Exception e)
			{
				Log.Error("ExecuteMirrorQuery", "Error executing query", e);
				throw;
			}
		}

		/// <summary>
		/// funcion adds to where  "c.Id in [id1,id2,...,idn]". It is used in mirror request to search for the items which we show to user
		/// </summary>
		/// <param name="query">query to add where clause</param>
		/// <param name="productIds"> items indentifications </param>
		/// <returns>query with "c.Id in [id1,id2,...,idn]" in where clause</returns>
		private static string BuildMirrorQuery(string query, int[] productIds)
		{
			if (productIds != null && productIds.Length > 0)
			{
				StringBuilder whereBuilder = new StringBuilder("c.Id IN [");
				for (int i = 0; i < productIds.Length; i++)
				{
					whereBuilder.Append(productIds[i].ToString() + ',');
				}

				whereBuilder.Remove(whereBuilder.Length - 1, 1);
				whereBuilder.Append(']');
				string where = whereBuilder.ToString();

				if (!string.IsNullOrEmpty(query) && query.Contains(AggregateFilter.Where))
				{
					int wherePos = query.LastIndexOf(AggregateFilter.Where);
					string left = query.Substring(0, wherePos);
					string right = query.Substring(wherePos + 6);
					return left + AggregateFilter.Where + where + " " + AggregateFilter.And + right;
				}

				throw new Exception("Query doesn't contain where clause, impossible to insert conditions");
			}

			throw new Exception("itemIds parameter is null or doesn't have any element");
		}
	}
}
