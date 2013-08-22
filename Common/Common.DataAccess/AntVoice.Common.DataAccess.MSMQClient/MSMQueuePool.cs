using AntVoice.Common.DataAccess.MSMQClient.Entities;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.MSMQClient
{
    public class MSMQueuePool<T> : IDisposable
        where T : class
    {
        public event QueueMessageReceivedHandler<T> QueueMessageReceived;
        protected virtual void OnQueueMessageReceived(QueueMessageReceivedEventArgs<T> e)
        {
            if (QueueMessageReceived != null)
                QueueMessageReceived(this, e);
        }

        private List<MSMQueueAccess<T>> _recipientClients;
        private List<Thread> _runningThreads = new List<Thread>();
        private int _nbClients;
        private string _queueName;
        private MessageQueue _queue;

        private object _syncRoot = new object();

        public bool IsListening { get; internal set; }

        private MSMQueuePool() { }

        #region Private methods

        private void InitializeQueues(int nbClient, string queueName)
        {
            _queueName = queueName;
            string queuePath = GetQueuePath();
            _nbClients = nbClient;

            if (!MessageQueue.Exists(queuePath))
            {
                MessageQueue.Create(queuePath);
            }

            if (nbClient <= 0)
                Log.Info("MSMQueuePool.Initialize", "0 clients declared - Listening to queue is disabled", _queueName);

            MessageQueue queue = new MessageQueue();
            queue.Path = queuePath;
            queue.DefaultPropertiesToSend.Recoverable = true;
            queue.DefaultPropertiesToSend.AttachSenderId = false;
            queue.Formatter = new XmlMessageFormatter(new[] { typeof(T) });

            SetQueue(queue);

            _recipientClients = new List<MSMQueueAccess<T>>(nbClient);
            for (int i = 0; i < nbClient; i++)
            {
                MSMQueueAccess<T> q = new MSMQueueAccess<T>(GetQueue(), this);
                q.QueueMessageReceived += new QueueMessageReceivedHandler<T>(queue_QueueMessageReceived);
                _recipientClients.Add(q);
            }
        }

        private MessageQueue GetQueue()
        {
            Monitor.Enter(_syncRoot);
            try
            {
                while (null == _queue)
                {
                    Monitor.Wait(_syncRoot);
                }
                return _queue;
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        private void SetQueue(MessageQueue queue)
        {
            Monitor.Enter(_syncRoot);
            try
            {
                _queue = queue;
                Monitor.PulseAll(_syncRoot);
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        private string GetQueuePath()
        {
            return string.Format(".\\private$\\{0}", _queueName.ToString());
        }

        #endregion

        public static MSMQueuePool<T> CreatePool(int nbClient, string queueName)
        {
            MSMQueuePool<T> result = new MSMQueuePool<T>();
            result.InitializeQueues(nbClient, queueName);
            return result;
        }

        #region Public methods

        public void Send(T item)
        {
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                Message message = new Message(item);
                message.Recoverable = false;
                GetQueue().Send(message);
            }
            catch (MessageQueueException mqe)
            {
                throw new Exception("Error sending message", mqe);
            }
            finally
            {
                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.MSMQ_Send, watch);
            }
        }

        public void BeginListening()
        {
            IsListening = true;
            for (int i = 0; i < _recipientClients.Count; i++)
            {
                Thread thread = new Thread(_recipientClients[i].Listen);
                thread.Name = string.Format("WORKER_{0}_{1}", _queueName.ToString(), i);
                thread.IsBackground = true;
                thread.Start();

                _runningThreads.Add(thread);
            }
        }

        public void RecreateQueue()
        {
            StopListening();

            string queueName = GetQueuePath();
            if (MessageQueue.Exists(queueName))
            {
                MessageQueue.Delete(queueName);
            }
            MessageQueue.Create(queueName);

            InitializeQueues(_nbClients, _queueName);
            BeginListening();
        }

        public void ReattachQueue()
        {
            StopListening();

            InitializeQueues(_nbClients, _queueName);

            BeginListening();
        }

        public void StopListening()
        {
            IsListening = false;
            Monitor.Enter(_syncRoot);
            try
            {
                if (_queue != null)
                {
                    _queue.Dispose();
                    _queue = null;
                    Monitor.PulseAll(_syncRoot);

                    foreach (Thread thread in _runningThreads)
                    {
                        thread.Join();
                    }
                    Logger.Current.Info("MSMQueuePool.StopListening", "All threads were stopped successfully");
                    _runningThreads.Clear();
                }
            }
            finally
            {
                Monitor.Exit(_syncRoot);
            }
        }

        #endregion

        #region Events

        void queue_QueueMessageReceived(object sender, QueueMessageReceivedEventArgs<T> e)
        {
            OnQueueMessageReceived(e);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _recipientClients.Clear();
            StopListening();
        }

        #endregion
    }
}
