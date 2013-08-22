using AngelList.DataEntities;
using Common.DataAccess.MongoClient;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataAccess
{
	public class IncubationDataAccess
	{
		#region Mongo collection name

		private const string FUND_INCUBATIONS_COLLECTION = "FundIncubations";

		#endregion

		private static readonly DataAccessSettings _settings = new DataAccessSettings();


		#region Select
		public static List<FundIncubation> FindExistingIncubations(List<int> ids)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var query = Query.In("Fund._id", ids.Select(id => new BsonInt32(id)));
			var res = _client.GetDocuments<FundIncubation>(query, FUND_INCUBATIONS_COLLECTION).ToList();

			Log.Debug("FindExistingIncubations", "Query", query);
			return res;
		}

		public static List<FundIncubation> GetAllFundIncubations()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var res = _client.GetDocuments<FundIncubation>(FUND_INCUBATIONS_COLLECTION).ToList();

			return res;
		}

		public static List<FundIncubation> GetNewFundIncubation(DateTime lastUpdate)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var query = Query.GT("LastUpdate", new BsonDateTime(lastUpdate));
			var res = _client.GetDocuments<FundIncubation>(query, FUND_INCUBATIONS_COLLECTION).ToList();

			return res;
		}
		#endregion

		#region Upsert
		public static void Update(FundIncubation incubation)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.SaveDocument<FundIncubation>(incubation, FUND_INCUBATIONS_COLLECTION);
		}

		public static void SaveDailyActivity(List<FundIncubation> incubations)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.InsertDocuments<FundIncubation>(incubations, FUND_INCUBATIONS_COLLECTION);
		}
		#endregion

		#region Delete
		public static void DeleteFundIncubationCollection()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.RemoveCollection(FUND_INCUBATIONS_COLLECTION);
		}
		#endregion
	}
}
