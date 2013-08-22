using AntVoice.Common.DataAccess.MSMQClient.Entities;
using AntVoice.Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.MSMQClient
{
    internal class MSMQueueAccess<T>
        where T : class
    {
        public event QueueMessageReceivedHandler<T> QueueMessageReceived;
        protected virtual void OnQueueMessageReceived(QueueMessageReceivedEventArgs<T> e)
        {
            if (QueueMessageReceived != null)
                QueueMessageReceived(this, e);
        }

        private MessageQueue _queue;
        private MSMQueuePool<T> _poolManager;

        public MSMQueueAccess(MessageQueue queue, MSMQueuePool<T> poolManager)
        {
            _queue = queue;
            _poolManager = poolManager;
        }

        public void Listen()
        {
            try
            {
                while (_poolManager.IsListening)
                {
                    try
                    {
                        using (Message message = _queue.Receive(TimeSpan.FromSeconds(1)))
                        {
                            try
                            {
                                if (message != null)
                                {
                                    T item = message.Body as T;
                                    if (item != null)
                                    {
                                        OnQueueMessageReceived(new QueueMessageReceivedEventArgs<T>(item));
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Current.Error("MSMQueueAccess.Listen", "Error getting message FROM MSMQ", e);
                            }
                        }
                    }
                    catch (MessageQueueException mqe)
                    {
                        // Handling timeout
                        if (mqe.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout)
                            throw mqe;
                    }
                }
            }
            catch (MessageQueueException mqe)
            {
                Logger.Current.Error("MSMQueueAccess.Listen", "Interrupting Message Queuing", mqe);
            }
            catch (Exception e)
            {
                Logger.Current.Error("MSMQueueAccess.Listen", "Error listening message FROM MSMQ", e);
            }
        }
    }
}
