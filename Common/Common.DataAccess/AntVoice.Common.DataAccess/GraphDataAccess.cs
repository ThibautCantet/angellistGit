using AntVoice.Common.DataAccess.NeoClient;
using AntVoice.Common.DataAccess.NeoClient.Contracts;
using AntVoice.Common.Entities.Neo;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess
{
	public class GraphDataAccess
	{
		public static bool SendData(IEnumerable<AntVoice.Common.Entities.Neo.GraphNode> nodes)
		{
			Neo4jContext _context = new Neo4jContext(true);

			string json = JsonConvert.SerializeObject(new { data = JsonConvert.SerializeObject(new { nodes = nodes }) });
			string result = _context.InsertData(json);

			try
			{
				InsertDataResult res = (InsertDataResult)JsonConvert.DeserializeObject(result, typeof(InsertDataResult));

				if (res.Status == "OK")
				{
					Log.Debug("NodeHandler.Handle", "The data insertion went fine");
					return true;
				}
				else
				{
					Log.Error("NodeHandler.Handle", "An error occured while inserting the data", res.Message);
					return false;
				}
			}
			catch (Exception e)
			{
				MonitoringTimers.Current.AddError(Counters.Neo4j_InsertDataPluginError);
				Log.Error("GraphDataAccess.SendData", "An error occured while deserializing the following Neo4j response", result);
			}

			return false;
		}
	}
}
