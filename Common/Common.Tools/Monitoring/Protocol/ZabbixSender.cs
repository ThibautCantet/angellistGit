using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Platform.Tools.Monitoring.Processors;
using Platform.Tools.Monitoring.TimeManagement;
using Platform.Tools.Logging;

namespace Platform.Tools.Monitoring.Protocol
{
    internal class ZabbixSender
	{
		private static byte[] ZABBIX_HEADER = new byte[] {
			(byte) 'Z',
			(byte) 'B',
			(byte) 'X',
			(byte) 'D',
			(byte) 0x01,
		};

		private const string JSON_HEADER =
			"{" +
			"\"request\":\"sender data\"," +
			"\"data\":["
			;
		private const string JSON_FOOTER = "]}";

		private int NumberOfItemsToSend = 5;

		private long MinInterval { get { return 100; } }
		private long DefaultInterval { get { return 500; } }

		private const long SENDER_INTERVAL = 60L * 1000L;

		private List<IProcessor> _processors = new List<IProcessor>() { new TimeMonitorProcessor() };
		
		public class ZabbixSenderProcessor
		{
			private long _lastRun = DateTime.Now.GetCurrentMillisecond();

			private ZabbixSender _sender;

			public ZabbixSenderProcessor()
			{
				_sender = new ZabbixSender();
			}

			public void Do()
			{
				while (true)
				{
					long nextRun = _lastRun + SENDER_INTERVAL;
					long sleepTime = nextRun - DateTime.Now.GetCurrentMillisecond();
					if (0 >= sleepTime)
					{
						sleepTime = SENDER_INTERVAL;
						nextRun = DateTime.Now.GetCurrentMillisecond() + sleepTime;
					}

					Thread.Sleep(Convert.ToInt32(sleepTime));

					_sender.ProcessItems();

					_lastRun = nextRun;
				}
			}
		}

		public static void StartProcess()
		{
			ZabbixSenderProcessor zsp = new ZabbixSenderProcessor();
			Thread thread = new Thread(zsp.Do);
			thread.Name = "ZabbixSender";
			thread.IsBackground = true;
			thread.Start();
		}

		public void ProcessItems()
		{
            if (!string.IsNullOrEmpty(ZabbixConfig.Current.Host) && !string.IsNullOrEmpty(ZabbixConfig.Current.ServerIP) && ZabbixConfig.Current.ServerPort > 0)
			{
				foreach (IProcessor p in _processors)
				{
					List<TrapperItem> items = p.GenerateItems();
                    SendItems(items, ZabbixConfig.Current.Host, ZabbixConfig.Current.ServerIP, ZabbixConfig.Current.ServerPort);
				}
			}
			else
			{
				Logger.Current.Error("ZabbixSender.ProcessItems", "Can't send metrics to Zabbix, maybe a configuration issue ?");
			}
		}

		private void SendItems(List<TrapperItem> items, string host, string serverIP, int serverPort)
		{
			if (null == items || 0 == items.Count)
			{
				return;
			}

			try
			{
				StringBuilder builder = new StringBuilder(JSON_HEADER);
				bool first = true;
				for (int i = 0; i < items.Count; i++)
				{
					var item = items[i];

					if (first)
					{
						first = false;
					}
					else
					{
						builder.Append(",\n");
					}

					string json = item.ToJson(host, ZabbixConfig.Current.ComponentName);
					builder.Append(json);

					if ((i + 1) % NumberOfItemsToSend == 0 || (i == items.Count - 1))
					{
						using (Socket socket = ConnectSocket(serverIP, serverPort))
						{
							builder.Append(JSON_FOOTER);
							socket.Send(ZABBIX_HEADER);
							int length = builder.Length;
							byte[] lengthArray = new byte[8];
							for (int j = 0; j < 8; j++)
							{
								lengthArray[j] = (byte)(length & 0xff);

								length >>= 8;
							}

							socket.Send(lengthArray);
							string content = builder.ToString();
							byte[] sent = Encoding.ASCII.GetBytes(content);
							socket.Send(sent);
							builder = new StringBuilder(JSON_HEADER);
							first = true;
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Current.Error("ZabbixSender.SendItems", "Error while sending messages to Zabbix", e);
			}
		}

		private Socket ConnectSocket(string serverIP, int serverPort)
		{
			Socket s = null;
			IPHostEntry hostEntry = null;

			// Get host related information.
			hostEntry = Dns.GetHostEntry(serverIP);

			// Loop through the AddressList to obtain the supported AddressFamily. This is to avoid
			// an exception that occurs when the host IP Address is not compatible with the address family
			// (typical in the IPv6 case).
			foreach (IPAddress address in hostEntry.AddressList)
			{
				IPEndPoint ipe = new IPEndPoint(address, serverPort);
				Socket tempSocket =
					new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

				tempSocket.Connect(ipe);

				if (tempSocket.Connected)
				{
					s = tempSocket;
					break;
				}
				else
				{
					continue;
				}
			}
			return s;
		}
	}
}
