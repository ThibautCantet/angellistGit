using AngelList.DataAccess;
using AngelList.DataEntities;
using AngelList.Entities.Feed;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class IncubationStream
	{
		public static void GetIncubationsFromFeed()
		{
			try
			{
				DateTime lastUpdate = ExecutionUpdater.GetLastUpdateIncubation();
				var feeds = FeedStream.GetStartupFeed();
				List<Feed> incubations = feeds.Where(f => f.item.typeValue == Entities.FeedTypes.StartupRole
					&& f.timestamp.ToDateTime() > lastUpdate
					&& !string.IsNullOrEmpty(f.description)
					&& f.description.Contains(Constants.INCUBATE)).ToList();

				var group = incubations.GroupBy(f => f.actor.id);

				Dictionary<Entity, List<Incubation>> incubationsFeedGrouped = new Dictionary<Entity, List<Incubation>>();
				foreach (var item in group)
				{
					incubationsFeedGrouped.Add(new Entity()
					{
						Id = item.First().actor.id,
						Name = item.First().actor.name,
						TagLine = item.First().actor.tagline,
						Url = item.First().actor.angellist_url
					}
					, (item.Select(incubation => new Incubation()
					{
						Id = incubation.id,
						FundId = incubation.actor.id,
						Startup = new Entity()
						{
							Id = incubation.target.id,
							Name = incubation.target.name,
							Url = incubation.target.angellist_url,
							TagLine = incubation.target.tagline,
						},
						Description = incubation.description,
						DateActivityFeed = incubation.timestamp
					})).ToList());
				}

				SaveDailyActivity(incubationsFeedGrouped);

				//	ExecutionUpdater.InsertFirst();
				ExecutionUpdater.UpdateIncubation();

				//	return incubationsFeedGrouped;
			}
			catch (Exception ex)
			{
				Log.Error("IncubationStream.GetIncubationsFromFeed", "error", ex);
			}
		}

		/// <summary>
		/// Used for the rss feed
		/// </summary>
		/// <returns>all the last funds' investments</returns>
		public static List<FundIncubation> GetNewFundIncubationList()
		{
			DateTime lastUpdate = ExecutionUpdater.GetLastDateRss();
			var fundIncubations = IncubationDataAccess.GetNewFundIncubation(lastUpdate);
			return fundIncubations;
		}

		/// <summary>
		/// Used for the rss feed
		/// </summary>
		/// <returns>all the last funds' investments</returns>
		public static List<FundIncubation> GetAllFundIncubationList()
		{
			return IncubationDataAccess.GetAllFundIncubations();
		}

		/// <summary>
		/// Used for the CRM
		/// </summary>
		/// <returns></returns>
		public static Dictionary<Entity, List<Incubation>> GetNewFundIncubation()
		{
			//	ExecutionUpdater.UpdateCrmSent();
			DateTime lastUpdate = ExecutionUpdater.GetLastDateCrmSent();
			List<FundIncubation> newFundIncubations = IncubationDataAccess.GetNewFundIncubation(lastUpdate);

			Dictionary<Entity, List<Incubation>> dic = new Dictionary<Entity, List<Incubation>>();

			foreach (var fund in newFundIncubations)
			{
				dic.Add(fund.Fund, fund.Incubations);
			}

			return dic;
		}

		public static Dictionary<Entity, List<Incubation>> GetHistory()
		{
			var fundIcubations = IncubationDataAccess.GetAllFundIncubations();

			Dictionary<Entity, List<Incubation>> gp = new Dictionary<Entity, List<Incubation>>();

			List<FundIncubation> update = new List<FundIncubation>();
			foreach (var fund in fundIcubations)
			{
				gp.Add(fund.Fund, fund.Incubations);
			}

			return gp;
		}

		public static Dictionary<Entity, List<Incubation>> CorrectFundIncubations()
		{
			var fundIncubations = IncubationDataAccess.GetAllFundIncubations();
			var toto = fundIncubations.GroupBy(f => f.Fund.Id);

			int count = toto.Count();

			if (fundIncubations.Count != count)
			{
				Log.Error("IncubationStream.GetHistory", "Duplicated fund");
			}

			Dictionary<Entity, List<Incubation>> gp = new Dictionary<Entity, List<Incubation>>();

			List<FundIncubation> update = new List<FundIncubation>();
			foreach (var fund in toto)
			{
				gp.Add(fund.First().Fund, fund.SelectMany(e => e.Incubations).ToList());
				var all = fund.SelectMany(e => e.Incubations).ToList();
				List<Incubation> distincIncubations = fund.SelectMany(e => e.Incubations).Distinct(new IncubationComparer()).ToList();
				if (all.Count() != distincIncubations.Count)
				{
					Console.WriteLine("distinct");
				}
				update.Add(new FundIncubation() { Fund = fund.First().Fund, Incubations = distincIncubations, LastUpdate = DateTime.UtcNow });
			}

			IncubationDataAccess.DeleteFundIncubationCollection();
			IncubationDataAccess.SaveDailyActivity(update);

			return gp;
		}

		private static void SaveDailyActivity(Dictionary<Entity, List<Incubation>> dic)
		{
			Dictionary<Entity, List<Incubation>> toSave = new Dictionary<Entity, List<Incubation>>();
			foreach (var item in dic)
			{
				toSave.Add(item.Key, item.Value);
			}
			#region Update FundIncubations existant
			try
			{
				List<FundIncubation> existingFundIncubations = IncubationDataAccess.FindExistingIncubations(toSave.Select(i => i.Key.Id).ToList());

				if (existingFundIncubations.Count > toSave.Count)
				{
					Log.Error("IncubationStream.SaveDailyActivity", "Too much FundIncubation found");
					return;
				}
				foreach (var fundIncubation in existingFundIncubations)
				{
					var current = toSave.FirstOrDefault(i => i.Key.Id == fundIncubation.Fund.Id);
					Entity fund = current.Key;
					List<Incubation> updatedIncubations = fundIncubation.Incubations;
					bool newIncubation = false;
					foreach (var invest in current.Value)
					{
						// vérifie que l'investissement courant n'existe pas dans l'investissement existant
						Incubation investement = fundIncubation.Incubations.FirstOrDefault(i => i.Id == invest.Id);
						if (investement == null)
						{
							updatedIncubations.Add(invest);
							newIncubation = true;
						}
						else
						{
							toSave.Remove(fund);
						}
					}
					if (newIncubation)
					{
						var updatedFundIncubation = new FundIncubation(fundIncubation, fund, updatedIncubations);
						IncubationDataAccess.Update(updatedFundIncubation);
						Log.Info("IncubationStream.SaveDailyActivity", "Update fund incubation",
							string.Format("{0} in {1}",
								updatedFundIncubation.Fund.Name,
								string.Join(", ", updatedFundIncubation.Incubations.Select(incubation => incubation.Startup.Name))));
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("IncubationStream.SaveDailyActivity", "Update", ex);
				throw;
			}
			#endregion

			#region Insert nouveau FundIncubations
			try
			{
				if (toSave.Any())
				{
					List<FundIncubation> incubations = new List<FundIncubation>();
					foreach (var item in toSave)
					{
						incubations.Add(new FundIncubation()
						{
							Fund = item.Key,
							Incubations = item.Value,
							LastUpdate = DateTime.UtcNow
						});
					}
					IncubationDataAccess.SaveDailyActivity(incubations);
					Log.Info("IncubationStream.SaveDailyActivity", "New funds incubations", string.Join(", ", incubations.ToString()));
				}
				else
				{
					Log.Info("IncubationStream.SaveDailyActivity", "No new fund incubation");
				}
			}
			catch (Exception ex)
			{
				Log.Error("IncubationStream.SaveDailyActivity", "Insert", ex);
				throw;
			}
			#endregion
		}
	}
}
