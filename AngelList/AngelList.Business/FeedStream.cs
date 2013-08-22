using AngelList.DataAccess;
using AngelList.Entities.Feed;
using Newtonsoft.Json;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Business
{
	public class FeedStream
	{
		internal static List<Feed> GetStartupFeed()
		{
			const string type = "Startup";
			var startups = GetFeeds().feed.Where(f => f.target != null && f.target.type == type).ToList();
			return startups;
		}

		private static Feeds GetFeeds()
		{
			try
			{
				Feeds feeds = GetFeeds(1);
				List<Feed> feedList = feeds.feed;

				for (int i = 2; i < feeds.last_page; i++)
				{
					var f = GetFeeds(i).feed;
					feedList.AddRange(f);
				}

				feeds.feed = feedList;

				return feeds;
			}
			catch (Exception ex)
			{
				Log.Error("GetFeeds", "error", ex);
				throw;
			}
		}

		private static Feeds GetFeeds(int id = 0)
		{
			string res = WebAccess.GetRequestResult(string.Format(@"https://api.angel.co/1/feed?page={0}", id));
			var feed = JsonConvert.DeserializeObject<Feeds>(res);

			return feed;
		}
	}
}
