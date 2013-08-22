using AngelList.DataAccess;
using AngelList.DataEntities;
using AngelList.Entities.Feed;
using AngelList.Entities.Startup;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class InvestmentStream
	{
		public static void GetInvestmentsFromFeed()
		{
			try
			{
				DateTime lastUpdate = ExecutionUpdater.GetLastUpdateInvestment();
				var feeds = FeedStream.GetStartupFeed();
				List<Feed> investments = feeds.Where(f => f.item.typeValue == Entities.FeedTypes.StartupRole
					&& f.timestamp.ToDateTime() > lastUpdate
					&& !string.IsNullOrEmpty(f.description) 
					&& f.description.Contains(Constants.INVEST_IN)).ToList();

				//var group = investments.GroupBy(f => f.actor).ToDictionary(k => k.Key.name, v => v.ToList());
				var group = investments.GroupBy(f => f.actor.id);

				Dictionary<Entity, List<Investment>> investmentsFeedGrouped = new Dictionary<Entity, List<Investment>>();
				foreach (var item in group)
				{
					investmentsFeedGrouped.Add(new Entity()
					{
						Id = item.First().actor.id,
						Name = item.First().actor.name,
						TagLine = item.First().actor.tagline,
						Url = item.First().actor.angellist_url
					}
					, (item.Select(investment => new Investment()
							{
								Id = investment.id,
								FundId = investment.actor.id,
								Startup = new Entity()
								{
									Id = investment.target.id,
									Name = investment.target.name,
									Url = investment.target.angellist_url,
									TagLine = investment.target.tagline,
								},
								Description = investment.description,
								DateActivityFeed = investment.timestamp
							})).ToList());
				}

				SaveDailyActivity(investmentsFeedGrouped);

			//	ExecutionUpdater.InsertFirst();
				ExecutionUpdater.UpdateInvestment();

			//	return investmentsFeedGrouped;
			}
			catch (Exception ex)
			{
				Log.Error("GetInvestmentsFromFeed", "error", ex);
			}
		}

		/// <summary>
		/// Used for the rss feed
		/// </summary>
		/// <returns>all the last funds' investments</returns>
		public static List<FundInvestment> GetNewFundInvestmentList()
		{
			DateTime lastUpdate = ExecutionUpdater.GetLastDateRss();
			var fundInvestments = InvestmentDataAccess.GetNewFundInvestments(lastUpdate);
			return fundInvestments;
		}

		public static List<FundInvestment> GetAllFundInvestmentList()
		{
			return InvestmentDataAccess.GetAllFundInvestment();
		}

		/// <summary>
		/// Used for the CRM
		/// </summary>
		/// <returns></returns>
		public static Dictionary<Entity, List<Investment>> GetNewFundInvestment()
		{
		//	ExecutionUpdater.UpdateCrmSent();
			DateTime lastUpdate = ExecutionUpdater.GetLastDateCrmSent();
			List<FundInvestment> newFundInvestments = InvestmentDataAccess.GetNewFundInvestments(lastUpdate);

			Dictionary<Entity, List<Investment>> dic = new Dictionary<Entity, List<Investment>>(); 

			foreach (var fund in newFundInvestments)
			{
				dic.Add(fund.Fund, fund.Investments);
			}

			return dic;
		}

		public static Dictionary<Entity, List<Investment>> GetHistory()
		{
			var fundInvestments = InvestmentDataAccess.GetAllFundInvestment();

			Dictionary<Entity, List<Investment>> gp = new Dictionary<Entity, List<Investment>>();

			List<FundInvestment> update = new List<FundInvestment>();
			foreach (var fund in fundInvestments)
			{
				gp.Add(fund.Fund, fund.Investments);
			}

			return gp;
		}

		public static Dictionary<Entity, List<Investment>> CorrectFundInvestments()
		{
			var fundInvestments = InvestmentDataAccess.GetAllFundInvestment();
			var toto = fundInvestments.GroupBy(f => f.Fund.Id);

			int count = toto.Count();

			if (fundInvestments.Count != count)
			{
				Log.Error("GetHistory", "Duplicated fund");
			}

			Dictionary<Entity, List<Investment>> gp = new Dictionary<Entity, List<Investment>>(); 
			
			List<FundInvestment> update = new List<FundInvestment>();
			foreach (var fund in toto)
			{
				gp.Add(fund.First().Fund, fund.SelectMany(e => e.Investments).ToList());
				var all = fund.SelectMany(e => e.Investments).ToList();
				List<Investment> distincInvestments = fund.SelectMany(e => e.Investments).Distinct(new InvestmentComparer()).ToList();
				if (all.Count() != distincInvestments.Count)
				{
					Console.WriteLine("distinct");
				}
				update.Add(new FundInvestment() { Fund = fund.First().Fund, Investments = distincInvestments, LastUpdate = fund.First().LastUpdate.HasValue ? fund.First().LastUpdate.Value : DateTime.UtcNow });
			}

			InvestmentDataAccess.DeleteFundInvestmentCollection();
			InvestmentDataAccess.SaveDailyActivity(update);

			return gp;
		}

		private static void SaveDailyActivity(Dictionary<Entity, List<Investment>> dic)
		{
			if (dic.Count == 0)
			{
				return;
			}
			Dictionary<Entity, List<Investment>> toSave = new Dictionary<Entity, List<Investment>>();
			foreach (var item in dic)
			{
				toSave.Add(item.Key, item.Value);
			}
			#region Update FundInvestment existant
			try
			{
				List<FundInvestment> existingFundInvestments = InvestmentDataAccess.FindExistingInvestments(toSave.Select(i => i.Key.Id).ToList());

				if (existingFundInvestments.Count > toSave.Count)
				{
					Log.Error("SaveDailyActivity", "Too much FundInvestment found");
					return;
				}
				foreach (var fundInvestment in existingFundInvestments)
				{
					var current = toSave.FirstOrDefault(i => i.Key.Id == fundInvestment.Fund.Id);
					Entity fund = current.Key;
					List<Investment> updatedInvestments = fundInvestment.Investments;
					bool newInvestment = false;
					foreach (var invest in current.Value)
					{
						// vérifie que l'investissement courant n'existe pas dans l'investissement existant
						Investment investement = fundInvestment.Investments.FirstOrDefault(i => i.Id == invest.Id);
						if (investement == null)
						{
							updatedInvestments.Add(invest);
							newInvestment = true;
						}
						else
						{
							toSave.Remove(fund);
						}
					}
					if (newInvestment)
					{
						var updatedFundInvestment = new FundInvestment(fundInvestment, fund, updatedInvestments);
						InvestmentDataAccess.Update(updatedFundInvestment);
						Log.Info("SaveDailyActivity", "Update fund investments", 
							string.Format("{0} in {1}", 
								updatedFundInvestment.Fund.Name, 
								string.Join(", ", updatedFundInvestment.Investments.Select(investment => investment.Startup.Name))));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("SaveDailyActivity", "Update", ex);
				throw;
			}
			#endregion

			#region Insert nouveau FundInvestment
			try
			{
				if (toSave.Any())
				{
					List<FundInvestment> investments = new List<FundInvestment>();
					foreach (var item in toSave)
					{
						investments.Add(new FundInvestment()
						{
							Fund = item.Key,
							Investments = item.Value,
							LastUpdate = DateTime.UtcNow
						});
					}
					InvestmentDataAccess.SaveDailyActivity(investments);
					Log.Info("SaveDailyActivity", "New funds investments", string.Join(", ", investments.ToString()));
				}
				else
				{
					Log.Info("SaveDailyActivity", "No new fund investment");
				}
			}
			catch (Exception ex)
			{
				Log.Error("SaveDailyActivity", "Insert", ex);
				throw;
			}
			#endregion
		}
	}
}
