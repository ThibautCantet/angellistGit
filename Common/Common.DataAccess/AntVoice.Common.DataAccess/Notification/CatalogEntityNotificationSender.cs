using System;
using System.Collections.Generic;
using System.Linq;
using AntVoice.Common.Entities.MSMQ.Graph;
using AntVoice.Common.DataAccess.MSMQClient;
using AntVoice.Common.Entities.MSMQ;
using AntVoice.Platform.Tools.Logging;

namespace AntVoice.DataCenter.ASF.Graph.Notification
{
    public class CatalogEntityNotificationSender
    {
        private const string GraphItemsUpdatedQueue = "Graph-ItemsUpdated";

        private static MSMQueuePool<Queuable<GraphItemsUpdated>> _queue;

        static CatalogEntityNotificationSender()
        {
            try
            {
                Logger.Current.Debug("CatalogEntityNotificationSender.Initialize", "Initializing queues");

                QueueInitializer.InitializeSender<Queuable<GraphItemsUpdated>>(ref _queue, GraphItemsUpdatedQueue);

                Logger.Current.Debug("CatalogEntityNotificationSender.Initialize", "Initialization succeeded");
            }
            catch (Exception e)
            {
                Logger.Current.Error("CatalogEntityNotificationSender.Initialize", "Error during queue initialization", e);
            }
        }

        /// <summary>
        /// Send an event to the graph from the entity (Catalog, FO, BO)
        /// </summary>
        /// <param name="list">list of objects to update in the graph database</param>
        /// <param name="projectId">current project id</param>
        public void Send(IEnumerable<Common.Entities.Recommendation.Product> products, int projectId)
        {
            GraphItemsUpdated itemUpdated = Convert(products, projectId);
            SendGraphItemsUpdated(itemUpdated);
        }

        public static void SendGraphItemsUpdated(GraphItemsUpdated graphEvent)
        {
            try
            {
                Log.Debug("SendGraphItemsUpdated", "Sending message over MSMQ");
                _queue.Send(new Queuable<GraphItemsUpdated>(graphEvent));
            }
            catch (Exception e)
            {
                Logger.Current.Error("SendGraphItemsUpdated", "Error sending message over MSMQ", e);
            }
        }

        /// <summary>
        /// Convert catalog entities to ItemUpdated
        /// </summary>
        /// <param name="list">catalog entity from ASF</param>
        /// <param name="projectId">current project id</param>
        /// <returns>ItemUpdated</returns>
        private GraphItemsUpdated Convert(IEnumerable<Common.Entities.Recommendation.Product> products, int projectId)
        {
            return new GraphItemsUpdated()
            {
                Items = ConvertToItems(products, projectId).ToArray()
            };
        }

        /// <summary>
        /// Convert catalog entities to Item
        /// </summary>
        /// <param name="list">catalog entity from ASF</param>
        /// <param name="projectId">current project id</param>
        /// <returns>Item instances</returns>
        private IEnumerable<GraphItem> ConvertToItems(IEnumerable<Common.Entities.Recommendation.Product> products, int projectId)
        {
            var projectArray = (new List<int>() { projectId }).ToArray();
            foreach (var elt in products)
            {
                List<GraphTag> tags = new List<GraphTag>();

                yield return new GraphItem(elt.Id,
                    elt.ClientProductId,
                    elt.CreationDate,
                    elt.LastUpdate.HasValue ? elt.LastUpdate.Value : DateTime.UtcNow,
                    elt.Label,
                    projectArray,
                    new List<string>(),
                    new GraphItemType((int)elt.ItemType, elt.ItemType.ToString(), string.Empty),
                    tags,
                    (GraphItemStatus)elt.ItemStatus,
                    elt.IsNew,
                    elt.IsPromotion,
                    elt.MustBeHighlighted,
                    elt.LoggedRequired,
                    elt.PublicNotation,
                    (decimal?)elt.ProfessionalNotation,
                    ((int)elt.WebSiteId).ToString(),
                    elt.LastUpdate,
                    elt.ExtraProperties ?? new Dictionary<string, string>());
            }
        }
    }
}
