using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.MSMQClient.Entities
{
    public class QueueMessageReceivedEventArgs<T> : EventArgs
    {
        public T Item { get; set; }

        public QueueMessageReceivedEventArgs(T item)
        {
            Item = item;
        }
    }

    public delegate void QueueMessageReceivedHandler<T>(object sender, QueueMessageReceivedEventArgs<T> e);
}
