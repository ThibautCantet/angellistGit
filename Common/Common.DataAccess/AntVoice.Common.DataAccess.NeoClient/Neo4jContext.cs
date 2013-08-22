// <copyright file="GraphGateway.cs" company="AntVoice">
// Copyright (c) AntVoice. All rights reserved.
// </copyright>

namespace AntVoice.Common.DataAccess.NeoClient
{
    using AntVoice.Common.DataAccess.NeoClient.Configuration;
    using AntVoice.Common.DataAccess.NeoClient.Contracts;
    using AntVoice.Common.DataAccess.NeoClient.Contracts.Gremlin;
    using AntVoice.Platform.Tools.Logging;
    using AntVoice.Platform.Tools.Monitoring;
    using AntVoice.Platform.Tools.Monitoring.Interfaces;
    using AntVoice.Platform.Tools.Pools;
    using AntVoice.Platform.Tools.Pools.Abstract;
    using Neo4jClient;
    using Neo4jClient.Cypher;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Provides gateway with neo4j database
    /// WARNING : 
    ///     - When a method is a Getxxx, it will read things on the slave server
    ///     - When a method is a Writexxx, it will write things on the master server
    /// </summary>
    public class Neo4jContext
    {
        private bool _isMaster;
        private int _timeout = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="Neo4jContext" /> class.
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="timeout">The timeout to use during rest query</param>

        public Neo4jContext(bool isMaster)
        {
            _isMaster = isMaster;
            _timeout = GraphConfiguration.Current.Timeout;
            InitializePool(GraphConfiguration.Current.Reader.ToString(), GraphConfiguration.Current.Writer.ToString());
        }

        #region get methods

        /// <summary>
        /// Gets the specified query
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public string GetWithCypherQuery(string query, IDictionary<string, string> parameters, out string requestString)
        {
            requestString = ToQuery(query, parameters);
            return this.GetWithCypherQuery(requestString);
        }

        /// <summary>
        /// Gets the specified query
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public string GetWithCypherQuery(string query, IDictionary<string, string> parameters)
        {
            string requestString;
            return this.GetWithCypherQuery(query, parameters, out requestString);
        }

        public string GetWithCypherQuery(string query)
        {
            GraphPoolableClient client = null;
            string requestString = query;
            try
            {
                client = GetClient();
                string uri = client.ServiceHost;
                var request = (HttpWebRequest)WebRequest.Create(uri + "/cypher");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json; stream=true";
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Timeout = _timeout;
                requestString = "{\"query\" : \"" + query + "\"}";
                Log.Debug("Neo4jContext.GetWithCypherQuery", "query", requestString);
                using (var stream = request.GetRequestStream())
                {
                    var bytes = Encoding.UTF8.GetBytes(requestString);
                    stream.Write(bytes, 0, bytes.Length);
                    using (var responseStream = request.GetResponse().GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            var responseContent = reader.ReadToEnd();
                            Log.Debug("Neo4jContext.GetWithCypherQuery", "responseContent", responseContent);
                            return responseContent;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogException("GetWithCypherQuery", e, requestString);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Gets the index of the node reference by on the slave server
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public GraphNodeResult GetNodeByIndex(string index, string key, long value)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var responseContent = GetObjectByPath("/index/node/" + index + "/" + key + "/" + value);
                var result = JsonConvert.DeserializeObject<IList<GraphNodeResult>>(responseContent).FirstOrDefault();
                return result;
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.GraphDatabase_GetNodeByIndex, sw);
            }
        }

        public string GetNode(string nodeReference)
        {
            return this.GetObjectByPath("/node/" + nodeReference);
        }

        public string GetObjectByPath(string path)
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                string uri = client.ServiceHost;
                var request = (HttpWebRequest)WebRequest.Create(uri + path);
                request.Method = "GET";
                request.Accept = "application/json";
                request.Timeout = _timeout;
                using (var responseStream = request.GetResponse().GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        var responseContent = reader.ReadToEnd();
                        return responseContent;
                    }
                }

            }
            catch (Exception e)
            {
                LogException("GetObjectByPath", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Gets the specified node references on the slave server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public IEnumerable<NodeReference<T>> GetNodeReferences<T>(string query)
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return ((IRawGraphClient)client.GetClient())
                    .ExecuteGetCypherResults<Node<T>>(new CypherQuery(query, null, CypherResultMode.Set))
                    .Select(t => t.Reference);
            }
            catch (Exception e)
            {
                LogException("GetNodeReferences", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Get the typed relationship starting from a node
        /// </summary>
        /// <param name="sourceRef">The source node reference</param>
        /// <param name="relType">The relationship type</param>
        /// <param name="directional">Tells if the relationship must be directional</param>
        /// <returns>The list of relationship of type relType</returns>
        public IEnumerable<GraphQueryResult> GetAllRelationships(string sourceRef, string relType, bool directional)
        {
            IEnumerable<GraphQueryResult> results = null;

            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var result = QueryBatch(
                    JsonConvert.SerializeObject(new GraphAction[]{
					new GraphAction
					{
						Id = 1,
						Method = Constants.HttpPost,
						To = Constants.ToCypher,
						Body = new GraphQuery
						{
							Query = string.Format("START n=node({0}) MATCH n-[r:{1}]-{2}m RETURN ID(r), TYPE(r), r.Label, r.CreationDate, ID(m)", sourceRef, relType, directional ? ">" : string.Empty),
							Params = new Dictionary<string,object>()
						}
					}
				}));

                if (!string.IsNullOrEmpty(result))
                {
                    results = JsonConvert.DeserializeObject<GraphQueryResult[]>(result);
                }
                else
                {
                    results = Enumerable.Empty<GraphQueryResult>();
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("GraphGateway.RelationshipExist", "Request error", e);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.GraphDatabase_GetAllRelationships, sw);
            }

            return results;
        }

        #endregion

        #region delete methods

        /// <summary>
        /// Deletes all on the master server
        /// </summary>
        /// <returns></returns>
        public bool DeleteAll()
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                client.GetClient()
                    .Cypher
                    .Start(new { n = All.Nodes })
                    .Match("n-[r?]-()")
                    .Delete("n, r")
                    .ExecuteWithoutResults();
                return true;
            }
            catch (Exception e)
            {
                LogException("DeleteAll", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Deletes the specified node reference on the master server
        /// </summary>
        /// <param name="nodeReference">The node reference.</param>
        /// <returns></returns>
        public bool Delete(long nodeReference)
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                client.GetClient()
                    .Cypher
                    .Start(new { n = nodeReference })
                    .Match("n-[r?]-()")
                    .Delete("n, r")
                    .ExecuteWithoutResults();
                return true;
            }
            catch (Exception e)
            {
                LogException("Delete", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        #endregion

        #region index management

        /// <summary>
        /// Deletes the index on the master server
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        public void DeleteIndex(string indexName)
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                if (client.GetClient().CheckIndexExists(indexName, IndexFor.Node))
                    client.GetClient().DeleteIndex(indexName, IndexFor.Node);
            }
            catch (Exception e)
            {
                LogException("DeleteIndex", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Creates the index on the master server
        /// </summary>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="dropIfExists">if set to <c>true</c> [drop if exists].</param>
        public void CreateIndex(string indexName, bool dropIfExists)
        {
            if (dropIfExists)
                DeleteIndex(indexName);

            var indexConfiguration = new IndexConfiguration
            {
                Provider = IndexProvider.lucene,
                Type = IndexType.fulltext
            };

            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                client.GetClient().CreateIndex(indexName, indexConfiguration, IndexFor.Node);
            }
            catch (Exception e)
            {
                LogException("CreateIndex", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        #endregion

        #region Insert methods

        /// <summary>
        /// Inserts the specified nodes on the master server
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public string ExecuteGremlinScript(Script script, out string requestString)
        {
            requestString = null;
            try
            {
                requestString = JsonConvert.SerializeObject(script);
                var result = ExecuteGremlinScript(requestString);
                Debug.WriteLine("Query ready. Lauching...");

				return result;
            }
            catch (Exception e)
            {
                Logger.Current.Error("GraphGateway.ExecuteGremlinScript", "Request error", e);
            }

			return null;
        }

        /// <summary>
        /// Inserts the specified nodes on the master server
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public string ExecuteGremlinScript(Script script)
        {
            string requestString;
            return this.ExecuteGremlinScript(script, out requestString);
        }

        private string ExecuteGremlinScript(string requestString)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                string uri = client.ServiceHost;
                var request = (HttpWebRequest)WebRequest.Create(uri + "/ext/GremlinPlugin/graphdb/execute_script");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json; stream=true";
                request.AutomaticDecompression = DecompressionMethods.GZip;
                request.Timeout = _timeout;

                using (var stream = request.GetRequestStream())
                {
                    var bytes = Encoding.UTF8.GetBytes(requestString);
                    stream.Write(bytes, 0, bytes.Length);

                    using (var responseStream = request.GetResponse().GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            string responseContent = reader.ReadToEnd();
                            Log.Debug("Neo4jContext.ExecuteGremlinScript", "response content", request.RequestUri, responseContent);
							return responseContent;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                LogException("ExecuteGremlinScript", e);
                InformAboutHostProblems(client, e);
                throw;
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Gremlin_ExecuteScript, sw);
			}
        }

		public string InsertData(string json)
		{
			IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
			GraphPoolableClient client = null;
			try
			{
				client = GetWriterClient();
				string uri = client.ServiceHost;
				var request = (HttpWebRequest)WebRequest.Create(uri + "/ext/Inserter/graphdb/insert");
				request.Method = "POST";
				request.ContentType = "application/json; charset=utf-8";
				request.Accept = "application/json; stream=true";
				request.AutomaticDecompression = DecompressionMethods.GZip;
				request.Timeout = System.Threading.Timeout.Infinite;

				using (var stream = request.GetRequestStream())
				{
					var bytes = Encoding.UTF8.GetBytes(json);
					stream.Write(bytes, 0, bytes.Length);

					using (var responseStream = request.GetResponse().GetResponseStream())
					{
						using (var reader = new StreamReader(responseStream))
						{
							Log.Debug("Neo4jContext.InsertData", "Data inserted");
							return reader.ReadToEnd();
						}
					}

				}
			}
			catch (Exception e)
			{
				LogException("ExecuteGremlinScript", e);
				InformAboutHostProblems(client, e);
				throw;
			}
			finally
			{
				sw.Stop();
				MonitoringTimers.Current.AddTime(Counters.Gremlin_ExecuteScript, sw);
			}
		}

        /// <summary>
        /// Inserts the specified nodes on the master
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <returns></returns>
        public GraphNodeResult[] InsertBatch(IEnumerable<GraphAction> nodes)
        {
            int tries = 0;
            Exception ex = null;
            while (tries < 5)
            {
                try
                {
                    tries++;
                    var requestString = JsonConvert.SerializeObject(nodes.ToArray());
                    Log.Debug("Neo4jContext.InsertBatch", "requestString", requestString);
                    var responseContent = QueryBatch(requestString);
                    return JsonConvert.DeserializeObject<GraphNodeResult[]>(responseContent);
                }
                catch (Exception e)
                {
                    ex = e;
                    Logger.Current.Error("GraphGateway.InsertBatch", "Request error", e);
                }
            }

            Logger.Current.Error("GraphGateway.InsertBatch", "After 5 attempts InsertBatch faild", JsonConvert.SerializeObject(nodes.ToArray()));
            throw ex;
        }

        public string QueryBatch(string requestString)
        {
            GraphPoolableClient client = null;
            string queryResult = string.Empty;
            try
            {
                client = GetClient();
                string uri = client.ServiceHost;
                var request = (HttpWebRequest)WebRequest.Create(uri + "/batch");
                request.Method = "POST";
                request.ContentType = "application/json; charset=utf-8";
                request.Accept = "application/json";
                request.AutomaticDecompression = DecompressionMethods.GZip;
                //request.Timeout = _timeout;

                using (var stream = request.GetRequestStream())
                {
                    var bytes = Encoding.UTF8.GetBytes(requestString);
                    stream.Write(bytes, 0, bytes.Length);

                    using (var responseStream = request.GetResponse().GetResponseStream())
                    {
                        using (var reader = new StreamReader(responseStream))
                        {
                            queryResult = reader.ReadToEnd();
                        }
                    }
                }

                return queryResult;
            }
            catch (Exception e)
            {
                LogException("QueryBatch", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Inserts the specified node on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node)
            where T : class
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return new NodeResult<T>(client.GetClient().Create(node, Enumerable.Empty<IRelationshipAllowingParticipantNode<T>>(), Enumerable.Empty<IndexEntry>()), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Inserts the specified node on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <param name="relationships">The relationships.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node, IEnumerable<IRelationshipAllowingParticipantNode<T>> relationships)
            where T : class
        {
            GraphPoolableClient client = GetClient();
            try
            {
                return new NodeResult<T>(client.GetClient().Create(node, relationships.ToArray(), Enumerable.Empty<IndexEntry>()), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Inserts the specified node on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <param name="relationships">The relationships.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node, IRelationshipAllowingParticipantNode<T> relationships)
            where T : class
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return new NodeResult<T>(client.GetClient().Create(node, new[] { relationships }, Enumerable.Empty<IndexEntry>()), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Inserts the specified node on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <param name="relationships">The relationships.</param>
        /// <param name="indexEntries">The index entries.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node, IRelationshipAllowingParticipantNode<T> relationships, IEnumerable<IndexEntry> indexEntries)
            where T : class
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return new NodeResult<T>(client.GetClient().Create(node, new[] { relationships }, indexEntries), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }


        /// <summary>
        /// Inserts the specified node on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <param name="indexEntries">The index entries.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node, IEnumerable<IndexEntry> indexEntries)
            where T : class
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return new NodeResult<T>(client.GetClient().Create(node, null, indexEntries), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        /// <summary>
        /// Creates the specified graph entry on the master server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node">The node.</param>
        /// <param name="relationships">The relationships.</param>
        /// <param name="indexes">The indexes.</param>
        /// <returns></returns>
        public NodeResult<T> Insert<T>(T node, IEnumerable<IRelationshipAllowingParticipantNode<T>> relationships, IEnumerable<IndexEntry> indexes)
            where T : class
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                return new NodeResult<T>(client.GetClient().Create(node, relationships, indexes), node);
            }
            catch (Exception e)
            {
                LogException("Insert", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }


        /// <summary>
        /// Creates the relationship on the master server
        /// </summary>
        /// <typeparam name="TNode">The type of the node.</typeparam>
        /// <typeparam name="TRelationship">The type of the relationship.</typeparam>
        /// <param name="nodeReference">The node reference.</param>
        /// <param name="relationshipAllowingParticipantNode">The relationship allowing participant node.</param>
        public void CreateRelationship<TNode, TRelationship>(NodeReference<TNode> nodeReference, TRelationship relationshipAllowingParticipantNode)
            where TRelationship : Relationship, IRelationshipAllowingSourceNode<TNode>
        {
            GraphPoolableClient client = null;
            try
            {
                client = GetClient();
                client.GetClient().CreateRelationship(nodeReference, relationshipAllowingParticipantNode);
            }
            catch (Exception e)
            {
                LogException("CreateRelationship", e);
                InformAboutHostProblems(client, e);
                throw;
            }
        }

        #endregion

        #region Private helper methods

        private void InitializePool(string readClientHostName, string writeClientHostName)
        {
            if (!ReadWritePool.IsPoolReady(Pools.Neo4j))
            {
                GraphPoolableClient reader = new GraphPoolableClient(readClientHostName);
                GraphPoolableClient writer = new GraphPoolableClient(writeClientHostName);
                ReadWritePool.AddReadHosts(Pools.Neo4j, new[] { reader });
                ReadWritePool.AddWriteHosts(Pools.Neo4j, new[] { writer });
            }
        }

        private GraphPoolableClient GetClient()
        {
            if (_isMaster)
            {
                return this.GetWriterClient();
            }
            else
            {
                return this.GetReaderClient();
            }
        }

        private GraphPoolableClient GetReaderClient()
        {
            IPoolable host;
            if (ReadWritePool.TryGetReadHost(Pools.Neo4j, out host))
            {
                return (GraphPoolableClient)host;
            }
            // No host found
            Logger.Current.Error("Neo4jContext.GetReaderClient", "No reader host found in neo4j pool");

            // monitoring, for now it is made with StopWatch because there is no other possibilities
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            sw.Stop();
            MonitoringTimers.Current.AddTime(Counters.Neo4j_Pool_GetReaderClientErrors, sw);

            throw new Exception("No reader host found in neo4j pool");
        }

        private GraphPoolableClient GetWriterClient()
        {
            IPoolable host;
            if (ReadWritePool.TryGetWriteHost(Pools.Neo4j, out host))
            {
                return (GraphPoolableClient)host;
            }
            // No host found
            Logger.Current.Error("Neo4jContext.GetWriterClient", "No writer host found in neo4j pool");

            // monitoring, for now it is made with StopWatch because there is no other possibilities
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            sw.Stop();
            MonitoringTimers.Current.AddTime(Counters.Neo4j_Pool_GetWriterClientErrors, sw);

            throw new Exception("No writer host found in neo4j pool");
        }

        private void LogException(string methodName, Exception ex, string request = null)
        {
            if (ex is WebException)
            {
                try
                {
                    string neoError = new StreamReader((ex as WebException).Response.GetResponseStream()).ReadToEnd();
                    ex.Data["Neo4jResponse"] = neoError;
                    if (neoError.Contains("org.neo4j.graphdb.NotFoundException"))
                    {
                        Log.Debug("Neo4jContext." + methodName, "Request error", ex, neoError, request);
                        return;
                    }

                    Logger.Current.Error("Neo4jContext." + methodName, "Request error", ex, neoError, request);
                }
                catch
                {
                    Logger.Current.Error("Neo4jContext." + methodName, "Request error", ex, request);
                }
            }
            else
            {
                Logger.Current.Error("Neo4jContext." + methodName, "Request error", ex, request);
            }
        }

        private void InformAboutHostProblems(IPoolable host, Exception ex)
        {
            if (host != null)
            {
                if (ex is WebException)
                {
                    WebException webEx = ex as WebException;
                    if (webEx.Status == WebExceptionStatus.ConnectFailure ||
                        webEx.Status == WebExceptionStatus.ServerProtocolViolation ||
                        webEx.Status == WebExceptionStatus.NameResolutionFailure ||
                        webEx.Status == WebExceptionStatus.Timeout)
                    {
                        ReadWritePool.SetHostUnavailable(Pools.Neo4j, host);
                        Logger.Current.Error("Neo4jContext.InformAboutHostProblems", "Host Unavailable", host.ServiceHost);
                    }
                    else
                    {
                        // do nothing, for future we can add logic of "dirty" hosts
                    }
                }
            }
        }

        #endregion

        public static string ToQuery(string queryText, IDictionary<string, string> queryParameters)
        {
            return queryParameters
                .Keys
                .Aggregate(queryText, (current, paramName) => current.Replace("{" + paramName + "}", queryParameters[paramName])).Replace("\r\n", "   ");
        }
    }
}
