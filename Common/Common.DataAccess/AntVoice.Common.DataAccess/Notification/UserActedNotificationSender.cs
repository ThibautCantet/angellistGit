using System;
using System.Collections.Generic;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Common.Entities.MSMQ.Graph;
using AntVoice.Common.DataAccess.MSMQClient;
using AntVoice.Common.Entities.MSMQ;

namespace AntVoice.DataCenter.ASF.Graph.Notification
{
    public class UserActedNotificationSender
    {
        private const string GraphUserActionQueueName = "Graph-UserAction";

        private static MSMQueuePool<Queuable<GraphUserActed>> _queue;

        static UserActedNotificationSender()
        {
            try
            {
                Logger.Current.Debug("UserActedNotificationSender.Initialize", "Initializing queues");

                QueueInitializer.InitializeSender<Queuable<GraphUserActed>>(ref _queue, GraphUserActionQueueName);

                Logger.Current.Debug("UserActedNotificationSender.Initialize", "Initialization succeeded");
            }
            catch (Exception e)
            {
                Logger.Current.Error("UserActedNotificationSender.Initialize", "Error during queue initialization", e);
            }
        }

        /// <summary>
        /// Send an action to the graph
        /// </summary>
        /// <param name="item">Item target</param>
        /// <param name="action">Action name</param>
        /// <param name="user">Member id who trigged the action</param>
        /// <param name="recoGuid">id of the recommendation (base d'apprentissage)</param>
        public void Send(List<Common.Entities.Recommendation.Product> products, string action, GraphUser user, Guid? recoGuid, DateTimeOffset date)
        {
            try
            {
                var items = WidgetDisplayedNotificationSender.MappingProductToItem(products);
                var userActed = new GraphUserActed()
                                    { 
                                        RecommendationId = recoGuid,
                                        ActionDate = date.UtcDateTime,
                                        ActionLabel = action,
                                        Item = items.ToArray(),
                                        UserId = user.UserId
                                    };

                Log.Debug("UserActedNotificationSender.Send", "Send UserActed", string.Format("user {0}, items {1}, action {2}, reco guid {3}", user.UserId, items, action, recoGuid));
                SendGraphUserActed(userActed);
            }
            catch(Exception ex)
            {
                Log.Error("UserActedNotificationSender.Send", "Error while sending user acted", ex);
            }
        }

        private static void SendGraphUserActed(GraphUserActed graphEvent)
        {
            try
            {
                Log.Debug("SendGraphUserActed", "Sending message over MSMQ");
                _queue.Send(new Queuable<GraphUserActed>(graphEvent));
            }
            catch (Exception e)
            {
                Logger.Current.Error("SendGraphUserActed", "Error sending message over MSMQ", e);
            }
        }
    }
}
