using AngelList.Business;
using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Mvc;

namespace AngelList.Web.Rss.Controllers
{
    public class RssController : Controller
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {
            return View();
        }


		//public ActionResult TestRssFeed()
		//{
		//	var items = new List<SyndicationItem>();
		//	for (int i = 0; i < 20; i++)
		//	{
		//		var item = new SyndicationItem()
		//		{
		//			Id = Guid.NewGuid().ToString(),
		//			Title = SyndicationContent.CreatePlaintextContent(String.Format("My Title {0}", Guid.NewGuid())),
		//			Content = SyndicationContent.CreateHtmlContent("Content The stuff."),
		//			PublishDate = DateTime.Now
		//		};
		//		item.Links.Add(SyndicationLink.CreateAlternateLink(new Uri("http://www.google.com")));//Nothing alternate about it. It is the MAIN link for the item.
		//		items.Add(item);
		//	}
			

		//	return new RssFeed(title: "Test rss",
		//					   items: items,
		//					   contentType: "application/rss+xml",
		//					   description: String.Format("rss de test  {0}", Guid.NewGuid()));

		//}

		public ActionResult RssFeed()
		{
			var fundInvestments = InvestmentStream.GetNewFundInvestmentList();
			var news = new List<SyndicationItem>();
			foreach (var fundInvestment in fundInvestments)
			{
				foreach (var investment in fundInvestment.Investments)
				{
					SyndicationItem newInvestment = new SyndicationItem()
					{
						Title = SyndicationContent.CreatePlaintextContent(string.Format("{0} {1} {2}", fundInvestment.Fund.Name, Constants.INVEST_IN, investment.Startup.Name)),
						Content = SyndicationContent.CreateHtmlContent(investment.Description),
						PublishDate = DateTime.Parse(investment.DateActivityFeed),
						
					};
					newInvestment.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(investment.Startup.Url)));
					news.Add(newInvestment);
				}
			}

			var fundIncubations = IncubationStream.GetNewFundIncubationList();
			foreach (var fundIncubation in fundIncubations)
			{
				foreach (var incubation in fundIncubation.Incubations)
				{
					SyndicationItem newInvestment = new SyndicationItem()
					{
						Title = SyndicationContent.CreatePlaintextContent(string.Format("{0} {1} {2}", fundIncubation.Fund.Name, Constants.INCUBATE, incubation.Startup.Name)),
						PublishDate = DateTime.Parse(incubation.DateActivityFeed),

					};
					newInvestment.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(incubation.Startup.Url)));
					news.Add(newInvestment);
				}
			}

			ExecutionUpdater.UpdateRss();

			Log.Info("RssFeed", "update");

			return new RssFeed(title: "Angel list feed",
							   items: news,
							   contentType: "application/rss+xml",
							   description: String.Format("Thibaut Cantet Angel list  {0}", Guid.NewGuid()));

		}

		public ActionResult RssAllFeeds()
		{
			var fundInvestments = InvestmentStream.GetAllFundInvestmentList();
			var news = new List<SyndicationItem>();
			foreach (var fundInvestment in fundInvestments)
			{
				foreach (var investment in fundInvestment.Investments)
				{
					SyndicationItem newInvestment = new SyndicationItem()
					{
						Title = SyndicationContent.CreatePlaintextContent(string.Format("{0} {1} {2}", fundInvestment.Fund.Name, Constants.INVEST_IN, investment.Startup.Name)),
						PublishDate = DateTime.Parse(investment.DateActivityFeed),

					};
					newInvestment.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(investment.Startup.Url)));
					news.Add(newInvestment);
				}
			}

			var fundIncubations = IncubationStream.GetNewFundIncubationList();
			foreach (var fundIncubation in fundIncubations)
			{
				foreach (var incubation in fundIncubation.Incubations)
				{
					SyndicationItem newInvestment = new SyndicationItem()
					{
						Title = SyndicationContent.CreatePlaintextContent(string.Format("{0} {1} {2}", fundIncubation.Fund.Name, Constants.INCUBATE, incubation.Startup.Name)),
						PublishDate = DateTime.Parse(incubation.DateActivityFeed),

					};
					newInvestment.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(incubation.Startup.Url)));
					news.Add(newInvestment);
				}
			}

			Log.Info("RssFeed", "all feeds");

			return new RssFeed(title: "Angel list all feeds",
							   items: news,
							   contentType: "application/rss+xml",
							   description: String.Format("Thibaut Cantet Angel all list  {0}", Guid.NewGuid()));

		}
    }
}
