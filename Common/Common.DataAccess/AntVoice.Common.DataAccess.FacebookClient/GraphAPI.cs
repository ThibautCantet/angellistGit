using AntVoice.Common.DataAccess.FacebookClient.Exceptions;
using AntVoice.Platform.Tools.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    internal sealed partial class GraphAPI
    {
        private const string _graphAPIBaseURL = "https://graph.facebook.com";
        
        internal static dynamic Get(string url, string accessToken = null)
        {
            return JsonConvert.DeserializeObject(GetRaw(url, accessToken));
        }
        internal static string GetRaw(string url, string accessToken = null)
        {
            string result = null;

            try
            {
                result = Requests.Get(
                    BuildRequestUrl(url, accessToken));

                if ("false" == result.ToLower())
                {
                    throw new Exception("Facebook sent 200 OK with \"false\" as content");
                }
            }
            catch (Exception e)
            {
                Exception resultException;
                HandleException(url, e, out resultException);
                throw resultException;
            }

            return result;
        }

        internal static dynamic Post(string url, IDictionary<string, string> parameters)
        {
            string result = null;

            try
            {
                result = Requests.Post(
                    BuildPostRequestUrl(url),
                    parameters);

                if ("false" == result.ToLower())
                {
                    throw new Exception("Facebook sent 200 OK with \"false\" as content");
                }
            }
            catch (Exception e)
            {
                Exception resultException;
                HandleException(url, e, out resultException);
                throw resultException;
            }

            return JsonConvert.DeserializeObject(result);
        }

        internal static dynamic Delete(string url, string accessToken)
        {
            string result = null;

            try
            {
                Requests.Delete(BuildRequestUrl(url, accessToken));

                if ("false" == result.ToLower())
                {
                    throw new Exception("Facebook sent 200 OK with \"false\" as content");
                }
            }
            catch (Exception e)
            {
                Exception resultException;
                HandleException(url, e, out resultException);
                throw resultException;
            }

            return JsonConvert.DeserializeObject(result);
        }

        private static string BuildRequestUrl(string url, string accessToken = null)
        {
            string result = string.Empty;

            url = HttpUtility.UrlDecode(url);

            if (url.StartsWith("https://graph.facebook.com/") ||
                url.StartsWith("http://graph.facebook.com/"))
            {
                result = url;
            }
            else
            {
                result = string.Format("{0}/{1}",
                    _graphAPIBaseURL,
                    url.TrimStart('/'));
            }

            if (accessToken != null && !url.Contains("access_token="))
            {
                result = string.Format("{0}{1}access_token={2}",
                    result,
                    result.Contains("?") ? "&" : "?",
                    HttpUtility.UrlEncode(accessToken));
            }

            return result;
        }
        private static string BuildPostRequestUrl(string url)
        {
            return string.Format("{0}/{1}",
                _graphAPIBaseURL,
                url.TrimStart('/'));
        }

        private static void HandleException(string url, Exception e, out Exception resultException)
        {
            resultException = null;
            string message = null;
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string type = null;

            if (e is WebException)
            {
                WebException webException = (WebException)e;
                HttpWebResponse response = (HttpWebResponse)webException.Response;

                message = response.GetBody();
                statusCode = response.StatusCode;

                dynamic jsonResult = JsonConvert.DeserializeObject(message);
                if (jsonResult != null && jsonResult.error != null)
                {
                    if (jsonResult.error.type != null)
                    {
                        type = jsonResult.error.type;
                    }
                    else
                    {
                        type = jsonResult.error;
                    }
                }

                switch (type)
                {
                    case "OAuthException":
                    case "190":
                        resultException = new FacebookOAuthException(type, message);
                        break;

                    default:
                        resultException = new FacebookApiException(type, message);
                        break;
                }
            }
            else
            {
                message = e.Message;
            }

            // replacing ; character since it's our delimiter
            url = url.Replace(";", "%3b");
            message = message.Replace(";", "%3b");

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0};{1};{2};{3}", (int)statusCode, statusCode, url, message);

            Logger.Current.Error("GraphAPI.HandleException", sb.ToString(), e);
        }
    }
}
