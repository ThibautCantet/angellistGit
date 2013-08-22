using System;
using System.Collections.Generic;
using System.Linq;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Common.Entities.MSMQ.Graph;
using AntVoice.Common.Entities.MSMQ.ASF.Analytics;
using AntVoice.Common.DataAccess.MSMQClient;
using AntVoice.Common.Entities.MSMQ;

namespace AntVoice.DataCenter.ASF.Graph.Notification
{
    public class WidgetDisplayedNotificationSender
    {
        private const string WidgetDisplayedQueue = "ASF-WidgetDisplayed";

        private static MSMQueuePool<Queuable<WidgetDisplayedEvent>> _queue;

        static WidgetDisplayedNotificationSender()
        {
            try
            {
                Logger.Current.Debug("WidgetDisplayedNotificationSender.Initialize", "Initializing queues");

                QueueInitializer.InitializeSender<Queuable<WidgetDisplayedEvent>>(ref _queue, WidgetDisplayedQueue);

                Logger.Current.Debug("WidgetDisplayedNotificationSender.Initialize", "Initialization succeeded");
            }
            catch (Exception e)
            {
                Logger.Current.Error("WidgetDisplayedNotificationSender.Initialize", "Error during queue initialization", e);
            }
        }

        /// <summary>
        /// Send a notification when a widget is displayed
        /// </summary>
        /// <param name="products"></param>
        /// <param name="recoGuid"></param>
        public void Send(List<Common.Entities.Recommendation.Product> products, int userId, Guid recoGuid)
        {
            try
            {
                var items = MappingProductToItem(products);
                var widgetDisplayed = new WidgetDisplayedEvent()
                {
                    Items = items.ToArray(),
                    RecommendationId = recoGuid,
                    DisplayTime = DateTime.UtcNow,
                    UserId = userId
                };

                SendWidgetDisplayedEvent(widgetDisplayed);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("WidgetDisplayedNotificationSender.Send", "Error sending message over MSMQ", ex);
            }
        }

        public static void SendWidgetDisplayedEvent(WidgetDisplayedEvent widgetEvent)
        {
            try
            {
                Log.Debug("SendWidgetDisplayedEvent", "Sending message over MSMQ");
                _queue.Send(new Queuable<WidgetDisplayedEvent>(widgetEvent));
            }
            catch (Exception e)
            {
                Logger.Current.Error("SendWidgetDisplayedEvent", "Error sending message over MSMQ", e);
            }
        }


        public static List<GraphItem> MappingProductToItem(List<Common.Entities.Recommendation.Product> products)
        {
            var items = new List<GraphItem>();
            if (products.Any())
            {
                foreach (var product in products)
                {
                    var type = new GraphItemType((int) product.ItemType, product.ItemType.ToString(),
                                                 product.ItemType.ToString());
                    var tags = new List<GraphTag>();

                    items.Add(new GraphItem(product.Id
                                            , product.ClientProductId
                                            , product.CreationDate
                                            , product.LastUpdate.HasValue ? product.LastUpdate.Value : DateTime.UtcNow
                                            , product.Label
                                            , (new List<int>() { (int)product.WebSiteId }).ToArray()
                                            , new List<string>()
                                            , type
                                            , tags
                                            , (GraphItemStatus) ((int) product.ItemStatus)
                                            , product.IsNew
                                            , product.IsPromotion
                                            , product.MustBeHighlighted
                                            , product.LoggedRequired
                                            , product.PublicNotation
                                            ,
                                            product.ProfessionalNotation.HasValue
                                                ? decimal.Parse(product.ProfessionalNotation.ToString())
                                                : decimal.Zero
                                            , ((int)product.WebSiteId).ToString()
                                            , product.LastUpdate
                                            , product.ExtraProperties ?? new Dictionary<string, string>()));
                }
            }
            else
            {
                Logger.Current.Error("MappingProductToItem", "Empty catalogEntities parameter", string.Empty);
            }
            return items;
        }
    }
}
