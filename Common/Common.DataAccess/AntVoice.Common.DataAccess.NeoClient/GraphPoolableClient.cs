using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Pools.Abstract;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntVoice.Common.DataAccess.NeoClient
{
    public class GraphPoolableClient : IPoolable
    {
        private const string GraphUriFormat = "http://{0}/db/data";

        private GraphClient _client;
        private string _host;

        public GraphPoolableClient(string host)
        {
            _host = string.Format(GraphUriFormat, host);
            _client = new GraphClient(new Uri(_host));
        }

        public GraphClient GetClient() { return _client; }

        public bool IsWorking()
        {
            try
            {
                _client.Connect();
            }
            catch (Exception e)
            {
                Logger.Current.Error("GraphClient.IsWorking", "Client ping is not working for host " + _host, e);
                return false;
            }
            return true;
        }

        public string ServiceHost
        {
            get { return _host; }
        }
    }
}
