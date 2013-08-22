using AngelList.DataEntities;
using Common.DataAccess.MongoClient;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataAccess
{
	public class ExecutionDataAccess
	{
		#region Mongo collection name

		private const string EXECUTIONS_COLLECTION = "Executions";

		#endregion

		private static readonly DataAccessSettings _settings = new DataAccessSettings();

		#region Select

		public static DateTime GetLastUpdateInvestment()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			return _client.GetDocument<Execution>(query, EXECUTIONS_COLLECTION, new string[] { "LastUpdateInvestment" }).LastUpdateInvestment;
		}

		public static DateTime GetLastUpdateIncubation()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			return _client.GetDocument<Execution>(query, EXECUTIONS_COLLECTION, new string[] { "LastUpdateIncubation" }).LastUpdateIncubation;
		}

		public static DateTime GetLastDateCrmSent()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			return _client.GetDocument<Execution>(query, EXECUTIONS_COLLECTION, new string[] { "LastDateCrmSent" }).LastDateCrmSent;
		}

		public static DateTime GetLastDateRss()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			return _client.GetDocument<Execution>(query, EXECUTIONS_COLLECTION, new string[] { "LastDateRss" }).LastDateRss;
		}

		private static Execution GetLastExecution()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			return _client.GetDocument<Execution>(query, EXECUTIONS_COLLECTION);
		}
		#endregion

		#region Upsert
		private static void InsertFirst(Execution exe)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.InsertDocument<Execution>(exe, EXECUTIONS_COLLECTION);
		}

		public static void UpdateInvestment()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			_client.UpdateDocument<Execution>(query, Update<Execution>.Set(e => e.LastUpdateInvestment, DateTime.UtcNow), EXECUTIONS_COLLECTION);
		}


		public static void UpdateIncubation()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			_client.UpdateDocument<Execution>(query, Update<Execution>.Set(e => e.LastUpdateIncubation, DateTime.UtcNow), EXECUTIONS_COLLECTION);
		}

		public static void UpdateCrmSent()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			_client.UpdateDocument<Execution>(query, Update<Execution>.Set(e => e.LastDateCrmSent, DateTime.UtcNow), EXECUTIONS_COLLECTION);
		}

		public static void UpdateRss()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			var query = Query.EQ("_id", 1);
			_client.UpdateDocument<Execution>(query, Update<Execution>.Set(e => e.LastDateRss, DateTime.UtcNow), EXECUTIONS_COLLECTION);
		}
		#endregion

	}
}
