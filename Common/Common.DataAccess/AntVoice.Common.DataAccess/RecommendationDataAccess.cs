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
	public class RecommendationDataAccess
	{
		private enum GremlinFormatKey
		{
			Filter,
			ContentTypeFilling,
			ContentTypeKey,
			FilterEnd,
			ContentTypeFields
		}

		public static CypherResult ExecuteRecommendationQuery(GraphAction graphAction, ContentTypeDistribution distribution, AggregateFilter filters, AlgoContext context, out string requestString)
		{
			try
			{
				if (graphAction.Query != null)
				{
					return ExecuteCypherRecommendation(graphAction.Query.QueryContent, distribution, filters, context, out requestString);
				}
				else if (graphAction.Script != null)
				{
					return ExecuteGremlinRecommendation(graphAction.Script.Script, distribution, filters, context, out requestString);
				}
				else
				{
					Log.Error("ExecuteRecommendationQuery", "This graph actions defines neither a Cypher query, nor a Gremlin query", graphAction.Id);
                    requestString = null;
					return null;
				}
			}
			catch (Exception e)
			{
				Log.Error("ExecuteRecommendationQuery", "Error executing query", e);
				throw;
			}
		}

		public static CypherResult ExecuteGremlinRecommendation(string query, ContentTypeDistribution distribution, AggregateFilter filters, AlgoContext context, out string requestString)
		{
			try
			{
				string filteredQuery = BuildGremlinQuery(query, filters, distribution);
				Log.Debug("ExecuteGremlinRecommendation", "Query built from parameters");

				Neo4jContext client = new Neo4jContext(false);
				Dictionary<string, object> parameters = new Dictionary<string, object>();
				foreach (RecommendationContext item in context.Context.Keys)
				{
					parameters["pMap" + item.ToString()] = context.Context[item];
				}

				Script gremlinScript = new Script
				{
					ScriptStr = filteredQuery,
					ParameterMap = parameters
				};

				string response = client.ExecuteGremlinScript(gremlinScript, out requestString);
				if (!string.IsNullOrEmpty(response))
				{
					return JsonConvert.DeserializeObject<CypherResult>(response);
				}
				else
				{
					return null;
				}
			}
			catch (Exception e)
			{
				Log.Error("ExecuteRecommendationQuery", "Error executing query", e);
				throw;
			}
		}

		public static string BuildGremlinQuery(string query, AggregateFilter filters, ContentTypeDistribution distribution)
		{
			IEnumerable<string> distribFieldNames = Enumerable.Empty<string>();
			if (distribution != null && distribution.Quotas != null && distribution.Quotas.Count > 0)
			{
				distribFieldNames = distribution.GetFieldNames();
			}

			List<string> queryParts = new List<string>();
			int distribCount = distribFieldNames.Count();
			int cursor = 0;
			foreach (GremlinFormatKey formatKey in Enum.GetValues(typeof(GremlinFormatKey)))
			{
				string formatKeyStr = "{" + formatKey.ToString() + "}";

				int formatKeyPos = query.IndexOf(formatKeyStr, cursor);
				if (formatKeyPos >= 0)
				{
					queryParts.Add(query.Substring(cursor, formatKeyPos - cursor));

					switch (formatKey)
					{
						case GremlinFormatKey.ContentTypeFields:
							if (distribCount > 0)
							{
								string columnsName = "," + string.Join(", ", distribFieldNames.Select(f => "'" + f + "'"));
								queryParts.Add(columnsName);
								Log.Debug("BuildGremlinQuery", "Name of the columns added in the CypherResult", columnsName);
							}
							break;
						case GremlinFormatKey.Filter:
							if (filters != null && filters.Filters.Count > 0)
							{
								string gremlinCondition = filters.ToGremlinConditions();
								queryParts.Add(gremlinCondition);
								Log.Debug("BuildGremlinQuery", "The if statement created for filtering the results", gremlinCondition);
							}
							break;
						case GremlinFormatKey.FilterEnd:
							if (filters != null && filters.Filters.Count > 0)
							{
								queryParts.Add("\n}\n");
							}
							break;
						case GremlinFormatKey.ContentTypeFilling:
							if (distribCount > 0)
							{
								string fillingValues = string.Join("\n", distribFieldNames.Select(f => "d" + f + " = endNode.getProperty('" + f + "', null);"));
								queryParts.Add(fillingValues);
								Log.Debug("BuildGremlinQuery", "The lines used to gather node data used by distribution", fillingValues);
							}
							break;
						case GremlinFormatKey.ContentTypeKey:
							if (distribCount > 0)
							{
								string mapKey = "+';'+" + string.Join("+';'+", distribFieldNames.Select(f => "d" + f));
								queryParts.Add(mapKey);
								Log.Debug("BuildGremlinQuery", "The key added to the HashMap to return all the values needed", mapKey);
							}
							break;
					}

					cursor = formatKeyPos + formatKeyStr.Length;
				}
			}

			queryParts.Add(query.Substring(cursor));
			return string.Concat(queryParts);
		}

		private static CypherResult ExecuteCypherRecommendation(string query, ContentTypeDistribution distribution, AggregateFilter filters, AlgoContext context, out string requestString)
		{
			try
			{
				Log.Debug("ExecuteRecommendationQuery", "Building query from parameters", query);
				string filteredQuery = BuildCypherRecommendationQuery(query, filters, distribution);
				Log.Debug("ExecuteRecommendationQuery", "Query built from parameters", filteredQuery);

				Neo4jContext client = new Neo4jContext(false);
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				foreach (RecommendationContext item in context.Context.Keys)
				{
					parameters[item.ToString()] = context.Context[item];
				}
				string response = client.GetWithCypherQuery(filteredQuery, parameters, out requestString);
				return JsonConvert.DeserializeObject<CypherResult>(response);
			}
			catch (Exception e)
			{
				Log.Error("ExecuteRecommendationQuery", "Error executing query", e);
				throw;
			}
		}

		private static string BuildCypherRecommendationQuery(string query, AggregateFilter filters, ContentTypeDistribution distribution)
		{
			string where = string.Empty;
			if (filters != null)
			{
				where = filters.ToConditions();
			}
			Log.Debug("BuildRecommendationQuery", "where", where);

			var filteredQueryText = query.Replace("\r\n", " ").Replace("\n", " ");

			if (!string.IsNullOrEmpty(where) && filteredQueryText.Contains(AggregateFilter.Where))
			{
				Log.Debug("BuildRecommendationQuery", "Query before where", filteredQueryText);
				int wherePos = filteredQueryText.LastIndexOf(AggregateFilter.Where);
				string left = filteredQueryText.Substring(0, wherePos);
				Log.Debug("BuildRecommendationQuery", "left", left);
				string right = filteredQueryText.Substring(wherePos + 6);
				Log.Debug("BuildRecommendationQuery", "right", right);
				filteredQueryText = left + AggregateFilter.Where + where + " " + AggregateFilter.And + right;
				Log.Debug("BuildRecommendationQuery", "Query with where clause", filteredQueryText);
			}

			// Added the distribution configured field in the queries
			if (distribution != null && distribution.Quotas != null && distribution.Quotas.Count > 0)
			{
				Log.Debug("BuildRecommendationQuery", "Query before replace for quota", filteredQueryText);
				filteredQueryText = filteredQueryText.Replace("RETURN", string.Format("RETURN {0},", string.Join(",", distribution.GetFieldNames().Select(f => string.Format("c.{0}? as {0}", f)))));
			}
			Log.Debug("BuildRecommendationQuery", "Query generated", filteredQueryText);
			return filteredQueryText;
		}
	}
}
