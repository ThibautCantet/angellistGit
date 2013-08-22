using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Platform.Tools.Monitoring.TimeManagement
{
	///<summary>
	/// This class helps to measure time used and number of calls for services.
	///</summary>
    internal class TimeMonitorRepository
	{
		private static ConcurrentQueue<Token> _tokenToProcess = new ConcurrentQueue<Token>();
		private static ConcurrentDictionary<string, TimeMonitor> _timeMonitors = new ConcurrentDictionary<string, TimeMonitor>();

		static TimeMonitorRepository()
		{
			Thread thread = new Thread(() =>
			{
				while (true)
				{
					Token token;
					if (_tokenToProcess.TryDequeue(out token))
					{
						TimeMonitor tm;
						if (!_timeMonitors.TryGetValue(token.Name, out tm))
						{
							tm = new TimeMonitor(token.Name);
							_timeMonitors[token.Name] = tm;
						}
						tm.AddTime(token.Duration);
					}
					else
					{
						Thread.Sleep(5);
					}
				}
			});
			thread.Name = "TokenProcessor";
			thread.IsBackground = true;
			thread.Start();
		}

		public static void ResetRepository()
		{
			_timeMonitors.Clear();
		}

		public static void SaveTime(string name, long elapsedNano)
		{
			Token token = new Token(name, elapsedNano);
			EnqueueToken(token);
		}

		public static TimeMonitor GetTimeMonitor(string name)
		{
			TimeMonitor result = null;
			if (_timeMonitors.TryGetValue(name, out result))
				return result;
			return result;
		}

		public static List<string> GetTimeMonitorNames()
		{
			return new List<string>(_timeMonitors.Keys);
		}

		private static void EnqueueToken(Token token)
		{
			_tokenToProcess.Enqueue(token);
		}

		public class Token
		{
			public string Name { get; set; }
			public long Duration { get; set; }

			public Token(string name, long duration)
			{
				Name = name;
				Duration = duration;
			}
		}
	}
}
