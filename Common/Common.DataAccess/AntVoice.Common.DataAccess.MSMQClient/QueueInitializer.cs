using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.MSMQClient
{
    public static class QueueInitializer
    {
        public static void InitializeSender<T>(ref MSMQueuePool<T> queue, string queueName)
            where T : class
        {
            if (queue != null && queue.IsListening)
                queue.StopListening();

            // Creating queue access for a sender (so 0 listening clients)
            queue = MSMQueuePool<T>.CreatePool(0, queueName);
        }
    }
}
