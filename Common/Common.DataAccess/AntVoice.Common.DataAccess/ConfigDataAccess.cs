using AntVoice.Common.DataAccess.SqlServerClient;
using AntVoice.Common.Entities.Settings;
using AntVoice.Common.Entities.Settings.Enums;
using AntVoice.Common.Entities.Settings.Recommendation;
using AntVoice.Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess
{
	public class ConfigDataAccess
	{
		private const string Mars_Database_Key = "MarsDatabase";

		public static CommonConfiguration GetRecommendationConfiguration()
        {
            SqlServerContext _client = new SqlServerContext(Mars_Database_Key);
            CommonConfiguration result = new CommonConfiguration();
            try
            {
                DbCommand command = _client.GetStoredProcCommand("ps_Config_GetCommonConfig");

                using (IDataReader reader = _client.ExecuteReader(command))
                {
                    #region Config_WebSite
                    while (reader.Read())
                    {
                        int webSiteId = SqlDataHelper.GetIntValue("WebSiteId", reader);
                        result.WebSitesValues.Add((WebSites)webSiteId,
                            new WebSite
                            {
                                WebSiteId = (WebSites)webSiteId,
                                Name = SqlDataHelper.GetStringValue("Name", reader),
                                SelectionMethodId = (SelectionMethods)SqlDataHelper.GetIntValue("SelectionMethodId", reader),
                                StrongestInteractionName = SqlDataHelper.GetStringValue("StrongestInteractionName", reader),
                                DownloadFriendsLikes = SqlDataHelper.GetBoolValue("DownloadFriendsLikes", reader)
                            });
                    }
                    #endregion

                    #region WebSiteAlgorithms
                    reader.NextResult();
                    while (reader.Read())
                    {
                        WebSites webSiteId = (WebSites)SqlDataHelper.GetIntValue("WebSiteId", reader);
                        int algorithmId = SqlDataHelper.GetIntValue("AlgorithmId", reader);

                        result.DeclareAlgorithm(webSiteId, algorithmId);
                    }

                    reader.NextResult();
                    #endregion

                    #region AlgorithmGraphActions
                    while (reader.Read())
                    {
                        int algorithmId = SqlDataHelper.GetIntValue("AlgorithmId", reader);
                        int graphActionId = SqlDataHelper.GetIntValue("GraphActionId", reader);

                        result.DeclareGraphAction(algorithmId, graphActionId);
                    }

                    reader.NextResult();
                    #endregion

                    #region AggregationFunctions
                    List<AggregationFunction> aggregationFunctions = new List<AggregationFunction>();
                    while (reader.Read())
                    {
                        AggregationFunction af = new AggregationFunction();
                        af.Id = SqlDataHelper.GetIntValue("Id", reader);
                        af.Name = SqlDataHelper.GetStringValue("Name", reader);

                        aggregationFunctions.Add(af);
                    }

                    reader.NextResult();
                    #endregion

                    #region ScoreFunctions
                    List<ScoreFunction> scoreFunctions = new List<ScoreFunction>();
                    while (reader.Read())
                    {
                        ScoreFunction sf = new ScoreFunction();
                        sf.Id = SqlDataHelper.GetIntValue("Id", reader);
                        sf.Name = SqlDataHelper.GetStringValue("Name", reader);

                        scoreFunctions.Add(sf);
                    }

                    reader.NextResult();
                    #endregion

                    #region ScoreFunctionParameters
                    while (reader.Read())
                    {
                        ScoreFunctionParameter sfp = new ScoreFunctionParameter();
                        sfp.Id = SqlDataHelper.GetIntValue("Id", reader);
                        sfp.Parameter2 = SqlDataHelper.GetStringValue("Parameter2", reader);
						sfp.Parameter1 = SqlDataHelper.GetStringValue("Parameter1", reader);
						sfp.Value = SqlDataHelper.GetDoubleValue("Value", reader);
						sfp.WebSiteId = SqlDataHelper.GetIntValue("WebSiteId", reader);

                        int graphActionId = SqlDataHelper.GetIntValue("GraphActionId", reader);
                        int algorithmId = SqlDataHelper.GetIntValue("AlgorithmId", reader);
                        WebSites webSiteId = (WebSites)SqlDataHelper.GetIntValue("WebSiteId", reader);

                        result.AddScoreFunctionParameterToGraphAction(webSiteId, algorithmId, graphActionId, sfp);
                    }

                    reader.NextResult();
                    #endregion

                    #region AggregationFunctionParameters
                    while (reader.Read())
                    {
                        AggregationFunctionParameter afp = new AggregationFunctionParameter();
                        afp.Id = SqlDataHelper.GetIntValue("Id", reader);
                        afp.Name = SqlDataHelper.GetStringValue("Name", reader);
                        afp.Value = SqlDataHelper.GetDoubleValue("Value", reader);

                        int algorithmId = SqlDataHelper.GetIntValue("AlgorithmId", reader);
                        WebSites webSiteId = (WebSites)SqlDataHelper.GetIntValue("WebSiteId", reader);

                        result.AddAggregationFunctionParameterToAlgorithm(webSiteId, algorithmId, afp);
                    }

					reader.NextResult();
					List<RecommendationQuery> queries = new List<RecommendationQuery>();
					while (reader.Read())
					{
						RecommendationQuery query = new RecommendationQuery();
						query.Id = SqlDataHelper.GetIntValue("Id", reader);
						query.QueryContent = SqlDataHelper.GetStringValue("Query", reader);

						queries.Add(query);
					}
					
					reader.NextResult();
					List<GremlinScript> scripts = new List<GremlinScript>();
					while (reader.Read())
					{
						GremlinScript script = new GremlinScript();
						script.Id = SqlDataHelper.GetIntValue("Id", reader);
						script.Script = SqlDataHelper.GetStringValue("Script", reader);

						scripts.Add(script);
					}

					reader.NextResult();
					List<MirrorQuery> mirrorQueries = new List<MirrorQuery>();
					while (reader.Read())
					{
						MirrorQuery query = new MirrorQuery();
						query.Id = SqlDataHelper.GetIntValue("Id", reader);
						query.QueryContent = SqlDataHelper.GetStringValue("MirrorQuery", reader);

						mirrorQueries.Add(query);
					}

					reader.NextResult();
					List<MirrorScripts> mirrorScripts = new List<MirrorScripts>();
					while (reader.Read())
					{
						MirrorScripts script = new MirrorScripts();
						script.Id = SqlDataHelper.GetIntValue("Id", reader);
						script.MirrorScript = SqlDataHelper.GetStringValue("MirrorScript", reader);

						mirrorScripts.Add(script);
					}

                    reader.NextResult();
                    #endregion

                    #region GraphActions
                    while (reader.Read())
                    {
                        int graphActionId = SqlDataHelper.GetIntValue("Id", reader);
                        string name = SqlDataHelper.GetStringValue("Name", reader);
                        int expiration = SqlDataHelper.GetIntValue("Expiration", reader);

						int? queryId = SqlDataHelper.GetNullableInt("QueryId", reader);
						int? scriptId = SqlDataHelper.GetNullableInt("GremlinScriptId", reader);
                        int scoreFunctionId = SqlDataHelper.GetIntValue("ScoreFunctionId", reader);
						int? mirrorQueryId = SqlDataHelper.GetNullableInt("MirrorQueryId", reader);
						int? mirrorScriptId = SqlDataHelper.GetNullableInt("MirrorScriptId", reader);
						RecommendationQuery query = queries.Find(q => q.Id == queryId);
						GremlinScript script = scripts.Find(s => s.Id == scriptId);
						ScoreFunction scoreFunction = scoreFunctions.Find(sf => sf.Id == scoreFunctionId);
						MirrorQuery mirrorQuery = null;
						MirrorScripts mirrorScript = null;
                        if (mirrorQueryId.HasValue)
                        {
                            mirrorQuery = mirrorQueries.Find(mq => mq.Id == mirrorQueryId.Value);
                        }
						if (mirrorScriptId.HasValue)
						{
							mirrorScript = mirrorScripts.Find(ms => ms.Id == mirrorScriptId.Value);
						}

                        foreach (GraphAction ga in result.GetGraphActions(graphActionId))
                        {
                            ga.Name = name;
                            ga.Expiration = expiration;
                            ga.Query = query;
							ga.Script = script;
                            ga.ScoreFunction = scoreFunction;
                            ga.MirrorQuery = mirrorQuery;
							ga.MirrorScript = mirrorScript;
                        }
                    }

                    reader.NextResult();
                    #endregion

                    #region Algorithms
                    while (reader.Read())
                    {
                        int algorithmId = SqlDataHelper.GetIntValue("Id", reader);
                        string name = SqlDataHelper.GetStringValue("Name", reader);
                        bool isStandardQuery = SqlDataHelper.GetBoolValue("IsStandardQuery", reader);
                        int aggregationFunctionId = SqlDataHelper.GetIntValue("AggregationFunctionId", reader);
                        AggregationFunction af = aggregationFunctions.Find(a => a.Id == aggregationFunctionId);

                        foreach (Algorithm algorithm in result.GetAlgorithms(algorithmId))
                        {
                            algorithm.Name = name;
                            algorithm.IsStandardQuery = isStandardQuery;
                            algorithm.AggregationFunction = af;
                        }
                    }

                    reader.NextResult();
                    #endregion

                    #region ContentTypeDistribution
                    while (reader.Read())
                    {
                        ContentTypeDistribution distribution = new ContentTypeDistribution();
                        distribution.Id = SqlDataHelper.GetIntValue("Id", reader);
                        distribution.Name = SqlDataHelper.GetStringValue("Name", reader);

                        result.Distributions[distribution.Id] = distribution;
                    }

                    reader.NextResult();
                    #endregion

                    #region ContentTypeQuota
                    while (reader.Read())
                    {
                        ContentTypeQuota quota = new ContentTypeQuota();
                        quota.Id = SqlDataHelper.GetIntValue("Id", reader);
                        quota.Name = SqlDataHelper.GetStringValue("Name", reader);
                        quota.Quota = SqlDataHelper.GetDoubleValue("Quota", reader);

                        int distributionId = SqlDataHelper.GetIntValue("DistributionId", reader);
                        result.AddQuotaToDistribution(distributionId, quota);
                    }

                    reader.NextResult();
                    #endregion

                    #region ContentTypeValues
                    while (reader.Read())
                    {
                        ContentTypeValues value = new ContentTypeValues();
                        value.Id = SqlDataHelper.GetIntValue("Id", reader);
                        value.Name = SqlDataHelper.GetStringValue("Name", reader);
                        value.MongoName = SqlDataHelper.GetStringValue("MongoName", reader);
                        value.IsString = SqlDataHelper.GetBoolValue("IsString", reader);
                        value.Value = SqlDataHelper.GetStringValue("Value", reader);

                        int quotaId = SqlDataHelper.GetIntValue("QuotaId", reader);
                        result.AddValueToQuota(quotaId, value);
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("GetRecommendationConfiguration", "Error loading configuration", e);
            }
            return result;
        }
	}
}
