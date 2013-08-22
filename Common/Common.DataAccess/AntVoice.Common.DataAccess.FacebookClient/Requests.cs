using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    public class Requests
    {
        public static string Get(string url)
        {
            string result = string.Empty;

            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Method = "GET";

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader responseStream = new StreamReader(response.GetResponseStream()))
                    {
                        result = responseStream.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        public static string Post(string url, IDictionary<string, string> parameters)
        {
            string result = string.Empty;

            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                // building POST parameters
                if (parameters != null && parameters.Count > 0)
                {
                    bool first = true;

                    StringBuilder sb = new StringBuilder();
                    foreach (string key in parameters.Keys)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            sb.Append("&");
                        }

                        if (!string.IsNullOrEmpty(parameters[key]))
                        {
                            string value = parameters[key];
                            string encodedValue = HttpUtility.UrlEncode(value, Encoding.UTF8);

                            sb.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}",
                                key, encodedValue);
                        }
                        else
                        {
                            sb.Append(key);
                        }
                    }

                    using (MemoryStream requestStream = new MemoryStream(
                        Encoding.UTF8.GetBytes(sb.ToString())))
                    {
                        request.ContentLength = requestStream.Length;

                        requestStream.CopyTo(request.GetRequestStream());
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }

                using (WebResponse response = request.GetResponse())
                {
                    using (StreamReader responseStream = new StreamReader(
                        response.GetResponseStream()))
                    {
                        result = responseStream.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return result;
        }

        public static void Delete(string url)
        {
            try
            {
                WebRequest request = HttpWebRequest.Create(url);
                request.Method = "DELETE";
                request.ContentLength = 0;

                using (WebResponse response = request.GetResponse())
                {
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
