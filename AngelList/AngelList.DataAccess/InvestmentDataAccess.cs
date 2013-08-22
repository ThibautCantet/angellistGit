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
	public class InvestmentDataAccess
	{
		#region Mongo collection name

		private const string FUND_INVESTMENTS_COLLECTION = "FundInvestments";

		#endregion

		private static readonly DataAccessSettings _settings = new DataAccessSettings();

		#region Select
		public static List<FundInvestment> FindExistingInvestments(List<int> ids)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var query = Query.In("Fund._id", ids.Select(id => new BsonInt32(id)));
			var res = _client.GetDocuments<FundInvestment>(query, FUND_INVESTMENTS_COLLECTION).ToList();

			Log.Debug("FindExistingInvestments", "Query", query);
			return res;
		}

		public static List<FundInvestment> GetAllFundInvestment()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var res = _client.GetDocuments<FundInvestment>(FUND_INVESTMENTS_COLLECTION).ToList();

			return res;
		}

		public static List<FundInvestment> GetNewFundInvestments(DateTime lastUpdate)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);
			var query = Query.GT("LastUpdate", new BsonDateTime(lastUpdate));
			var res = _client.GetDocuments<FundInvestment>(query, FUND_INVESTMENTS_COLLECTION).ToList();

			return res;
		}
		#endregion

		#region Upsert
		public static void Update(FundInvestment investement)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.SaveDocument<FundInvestment>(investement, FUND_INVESTMENTS_COLLECTION);
		}

		public static void SaveDailyActivity(List<FundInvestment> investements)
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.InsertDocuments<FundInvestment>(investements, FUND_INVESTMENTS_COLLECTION);
		}
		#endregion

		#region Delete
		public static void DeleteFundInvestmentCollection()
		{
			MongoDBContext _client = new MongoDBContext(_settings.MongoDBConnectionString);

			_client.RemoveCollection(FUND_INVESTMENTS_COLLECTION);
		}
		#endregion
	}
}
