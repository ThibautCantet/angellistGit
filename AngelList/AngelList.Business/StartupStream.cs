using AngelList.DataAccess;
using AngelList.Entities.Startup;
using Newtonsoft.Json;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class StartupStream
	{
		public static List<Startup> GetStartups()
		{
			var startupFeeds = FeedStream.GetStartupFeed();
			var startups = GetStartups(startupFeeds.Select(s => s.target.id));
			return startups;
		}

		private static List<Startup> GetStartups(IEnumerable<int> id)
		{
			try
			{
				const int MAX = 50;
				if (id.Count() > MAX)
				{
					int nbLoop = id.Count() / MAX;
					bool addLoop = id.Count() % MAX != 0;
					if (addLoop)
					{
						nbLoop++;
					}
					List<Startup> feeds = new List<Startup>();
					for (int i = 0; i < nbLoop; i++)
					{
						string res = WebAccess.GetRequestResult(@"https://api.angel.co/1/startups/batch?ids=" + string.Join(",", id.Skip(i*MAX).Take(MAX)));
						List<Startup> feed = JsonConvert.DeserializeObject<List<Startup>>(res);
						feeds.AddRange(feed);
					}
					return feeds;
				}
				else
				{
					string res = WebAccess.GetRequestResult(@"https://api.angel.co/1/startups/batch?ids=" + string.Join(",", id));
					var feed = JsonConvert.DeserializeObject<List<Startup>>(res);
					return feed;
				}
			}
			catch (Exception ex)
			{
				Log.Error("GetStartups", "error", ex);
				throw;
			}
		}
	}
}
