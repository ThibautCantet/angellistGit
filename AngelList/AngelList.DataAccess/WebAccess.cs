using Platform.Tools.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngelList.DataAccess
{
    public class WebAccess
    {
		/// <summary>
		/// get result of the API request
		/// </summary>
		/// <param name="url"> </param>
		/// <returns></returns>
		public static string GetRequestResult(string url)
		{
			try
			{
				using (WebClient myWebClient = new WebClient())
				{
					// Open a stream to point to the data stream coming from the Web resource.
					using (Stream myStream = myWebClient.OpenRead(url))
					{
						using (StreamReader sr = new StreamReader(myStream))
						{
							string r = (sr.ReadToEnd());

							// Close the stream.
							myStream.Close();
							Log.Debug("GetRequestResult", "json stream", url, r);
							return r;
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}
    }
}
