using Common.DataAccess.MongoClient;
using Platform.Tools.Logging;
using Platform.Tools.Monitoring;
using Platform.Tools.Monitoring.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.DataAccess.MongoClient
{
    /// <summary>
    /// Context for MongoDb
    /// </summary>
    public class MongoDBContext
    {
        /// <summary>
        /// MongoDB data base (instance)
        /// </summary>
        private readonly MongoDatabase _database;

        /// <summary>
        /// MongoDB data base
        /// </summary>
        private readonly MongoServer _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoGateway" /> class.
        /// </summary>
        /// <param name="serverName">Name of the server should be lke this "mongodb://localhost".</param>
        /// <param name="databaseName">Name of the database.</param>
        public MongoDBContext(string connectionString)
        {
			var databaseName = MongoUrl.Create(connectionString).DatabaseName;
            var mongoClient = new MongoDB.Driver.MongoClient(connectionString);
            _server = mongoClient.GetServer();            
            _database = _server.GetDatabase(databaseName);
        }

		public int GenerateId<T>(T document, string collectionName)
		{
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                return (int)IntIdGenerator.Instance.GenerateId(GetCollectionToRead<T>(collectionName), document);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_GenerateId, sw);
            }
		}

        public List<int> GenerateIds(string collectionName, int count)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                int last = (int)IntIdGenerator.Instance.UpdateId(GetCollectionToRead<BsonDocument>(collectionName), count);
                List<int> result = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    result.Add(last - i);
                }

                return result;
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_GenerateId, sw);
            }
        }

        public void InsertDocument<T>(T document, string collectionName)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
                collection.Insert(document);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Insert, sw);
            }
        }

        public void InsertDocuments<T>(IEnumerable<T> document, string collectionName)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
                collection.InsertBatch(document);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Insert, sw);
            }
        }

		/// <summary>
		/// Tells if a document exists based on the request given as parameteres
		/// </summary>
		/// <typeparam name="T">The type of the collection searched</typeparam>
		/// <param name="query">The request to execute</param>
		/// <param name="collectionName">The collection to search</param>
		/// <returns>Treu if the entry does exist</returns>
		public bool IsDocumentExist<T>(IMongoQuery query, string collectionName)
		{
			IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
			try
			{
				var collection = this.GetCollectionToRead<T>(collectionName);
				return collection.Count(query) > 0;
			}
			catch (Exception ex)
			{
				Logger.Current.Error("GetDocument", "Fail to get document", ex, collectionName, query);
				throw;
			}
			finally
			{
				sw.Stop();
				MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
			}
		}

		public T GetDocument<T>(IMongoQuery query, string collectionName)
		{
			IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
			try
			{
				var collection = this.GetCollectionToRead<T>(collectionName);
				return collection.FindAs<T>(query).FirstOrDefault();
			}
			catch (Exception ex)
			{
				Logger.Current.Error("GetDocument", "Fail to get document", ex, collectionName, query);
				throw;
			}
			finally
			{
				sw.Stop();
				MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
			}
		}

        public T GetDocument<T>(IMongoQuery query, string collectionName, string[] fields)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToRead<T>(collectionName);
                var cursor = collection.Find(query);
                cursor.SetFields(fields);
                return cursor.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("GetDocument", "Fail to get document", ex, collectionName, query);
                throw;
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
            }
        }
		/// <summary>
		/// Filter elements of a collection
		/// </summary>
		/// <typeparam name="T">type of objects in the collection</typeparam>
		/// <param name="query">filter</param>
		/// <param name="collectionName">collectionName</param>
		/// <returns>elements filtered</returns>
		public IEnumerable<T> GetDocuments<T>(string collectionName)
		{
			IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
			try
			{
				var collection = this.GetCollectionToRead<T>(collectionName);
				return collection.FindAll().AsEnumerable();
			}
			finally
			{
				sw.Stop();
				MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
			}
		}

        /// <summary>
        /// Filter elements of a collection
        /// </summary>
        /// <typeparam name="T">type of objects in the collection</typeparam>
        /// <param name="query">filter</param>
        /// <param name="collectionName">collectionName</param>
        /// <returns>elements filtered</returns>
        public IEnumerable<T> GetDocuments<T>(IMongoQuery query, string collectionName)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToRead<T>(collectionName);
                return collection.FindAs<T>(query).AsEnumerable();
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
            }
        }

        public List<T> GetDocuments<T>(IMongoQuery query, string collectionName, string[] fields)
        {
            return this.GetDocuments<T>(query, collectionName, fields, Int32.MaxValue);
        }


        /// <summary>
        /// gets the documents with specified fields returned from server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public List<T> GetDocuments<T>(IMongoQuery query, string collectionName, string[] fields, int limit)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToRead<T>(collectionName);
                var cursor = collection.Find(query);
                cursor.Limit = limit;
                cursor.SetFields(MongoDB.Driver.Builders.Fields.Include(fields));
                return cursor.ToList();
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Get_AllDocumentsWithQuery, sw);
            }
        }

        /// <summary>
        /// gets the documents sorted with specified fields returned from server
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public List<T> GetDocuments<T>(IMongoQuery query, string collectionName, string[] fields, IMongoSortBy sort, int limit)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToRead<T>(collectionName);
                var cursor = collection.Find(query).SetSortOrder(sort);
                cursor.Limit = limit;
                cursor.SetFields(MongoDB.Driver.Builders.Fields.Include(fields));
                return cursor.ToList();
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Get, sw);
            }
        }

        /// <summary>
		/// Update the first document found based on the IMongoUpdate query
		/// </summary>
		/// <typeparam name="T">The document type</typeparam>
		/// <param name="query">The query to define the document updated</param>
		/// <param name="update">The field to update</param>
		/// <param name="collectionName">The collection name</param>
		public void UpdateDocument<T>(IMongoQuery query, IMongoUpdate update, string collectionName)
		{
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
				collection.Update(query, update);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Update, sw);
            }
		}

        /// <summary>
		/// Update all document found, based on the IMongoUpdate query
		/// </summary>
		/// <typeparam name="T">The document type</typeparam>
		/// <param name="query">The query to define the document updated</param>
		/// <param name="update">The field to update</param>
		/// <param name="collectionName">The collection name</param>
		public void UpdateDocuments<T>(IMongoQuery query, IMongoUpdate update, string collectionName)
		{
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
				collection.Update(query, update, UpdateFlags.Multi);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Update, sw);
            }
		}

        /// <summary>
        /// to use this method document should have Id field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="document"></param>
        /// <param name="collectionName"></param>
        public void SaveDocument<T>(T document, string collectionName)
        {
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
                collection.Save<T>(document);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Save, sw);
            }
        }

        /// <summary>
        /// gets a document, change him with function and save
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="collectionName"></param>
        /// <param name="function"></param>
        public void EditDocument<T>(IMongoQuery query, string collectionName, Action<T> function)
        {
            using (_server.RequestStart(_database))
            {
                T doc = this.GetDocument<T>(query, collectionName);
                if (doc == null)
                {
                    Log.Error("MongoDBContext.EditDocument", "No document found", query, collectionName);
                }
                else
                {
                    function(doc);
                    this.SaveDocument<T>(doc, collectionName);
                }
            }
        }

		/// <summary>
		/// Remove a document based on a Mongo query
		/// </summary>
		/// <typeparam name="T">The document type to remove</typeparam>
		/// <param name="query">The query to selet the document to remove</param>
		/// <param name="collectionName">The collection to affect</param>
		public void RemoveDocument<T>(IMongoQuery query, string collectionName)
		{
            IStopwatch sw = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                var collection = this.GetCollectionToWrite<T>(collectionName);
                collection.Remove(query);
            }
            finally
            {
                sw.Stop();
                MonitoringTimers.Current.AddTime(Counters.Mongo_Remove, sw);
            }
		}

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        private MongoCollection<T> GetCollectionToWrite<T>(string collectionName)
        {
            var collection = _database.GetCollection<T>(collectionName, WriteConcern.Acknowledged);
            return collection;
        }

        /// <summary>
        /// Gets the collection.
        /// </summary>
        /// <typeparam name="T">The type of the document.</typeparam>
        /// <param name="collectionName">Name of the collection.</param>
        /// <returns></returns>
        private MongoCollection<T> GetCollectionToRead<T>(string collectionName)
        {
            var collection = _database.GetCollection<T>(collectionName, WriteConcern.Unacknowledged);
            return collection;
        }

        /// <summary>
        /// Removes the collection.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveCollection(string name)
        {
            var mongoCollection = _database.GetCollection(name);
            if (mongoCollection != null)
            {
                var indexesResult = mongoCollection.GetIndexes();
                if (indexesResult.Count > 0)
                {
                    var commandResult = mongoCollection.DropAllIndexes();
                    if (!commandResult.Ok)
                    {
                        return false;
                    }
                }
                if (mongoCollection.Exists())
                {
                    var result = mongoCollection.Drop();
                    if (!result.Ok)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
