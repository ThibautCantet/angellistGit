using AntVoice.Common.DataAccess.FacebookClient.Entities;
using AntVoice.Common.DataAccess.FacebookClient.Exceptions;
using AntVoice.Common.Entities.Users.Facebook;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Misc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    public class Authentication
    {
        /// <summary>
        /// Parses the signed_request sent by Facebook
        /// </summary>
        public static bool TryParseSignedRequest(string signedRequest, string secret, out FacebookSignedRequest parsedSignedRequest)
        {
            parsedSignedRequest = new FacebookSignedRequest();

            try
            {
                parsedSignedRequest.OriginalSignedRequest = signedRequest;
                string[] split = signedRequest.Split('.');
                string encodedSig = split[0];
                string encodedPayload = split[1];

                string payload = Encoding.UTF8.GetString(Crypto.Base64.UrlDecodeBase64(split[1]));
                parsedSignedRequest = JsonConvert.DeserializeObject<FacebookSignedRequest>(payload);

                if (parsedSignedRequest.Algorithm != "HMAC-SHA256")
                {
                    throw new Exception("Unknown algorithm. Expected HMAC-SHA256");
                }

                byte[] key = Encoding.UTF8.GetBytes(secret);
                byte[] digest = Crypto.SHA256.ComputeHMACSHA256(Encoding.UTF8.GetBytes(encodedPayload), key);

                if (!digest.SequenceEqual(Crypto.Base64.UrlDecodeBase64(encodedSig)))
                {
                    string d = string.Empty;
                    foreach (var val in digest)
                    {
                        d += val;
                    }
                    string e = string.Empty;
                    foreach (var val in Crypto.Base64.UrlDecodeBase64(encodedSig))
	                {
                        e += val;
                    }
                    Log.Error("Authentication.TryParseSignedRequest", "Bad Signed JSON signature", string.Format("encodedSig : {0}, encodedPayload : {1}, key : {2} not equals to digest : {3}", encodedSig, encodedPayload, d, e));
                    throw new Exception("Bad Signed JSON signature");
                }

                Logger.Current.Debug("Authentication.TryParseSignedRequest", "Signature ok", signedRequest, secret);
                return true;
            }
            catch (Exception e)
            {
                Logger.Current.Error("Authentication.TryParseSignedRequest", "Invalid signed_request", e, signedRequest, secret);
            }

            return false;
        }

        const string RenewAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri=&client_secret={1}&code={2}";
        /// <summary>
        /// Renew a user's Facebook access_token using the "code" parameter passed by Facebook
        /// </summary>
        public static FacebookOAuthResult<string> ExchangeCodeForAccessToken(string applicationId, string secretKey, string code, out DateTime expires)
        {
            FacebookOAuthResult<string> result = new FacebookOAuthResult<string>();
            expires = DateTime.MinValue;

            string url = string.Format(RenewAccessTokenURL,
                applicationId,
                secretKey,
                code);
            try
            {

                Logger.Current.Debug("ExchangeCodeForAccessToken", "Getting new access_token", url);

                string s = GraphAPI.GetRaw(url);
                if (s.StartsWith("access_token="))
                {
                    string[] split = s.Split('&');
                    string accessToken = split[0].Substring(split[0].IndexOf('=') + 1);
                    double expiresInSeconds = double.Parse(split[1].Substring(split[1].IndexOf('=') + 1));
                    expires = DateTime.UtcNow.AddSeconds(expiresInSeconds);

                    result.Data = accessToken;
                }
            }
            catch (FacebookOAuthException fbe)
            {
                Logger.Current.Error("ExchangeCodeForAccessToken", "Error getting new access_token", fbe, url);

                result.SetHasExpired();
            }
            catch (Exception e)
            {
                Logger.Current.Error("Authentication.RenewAccessToken", "Could not renew the user's access_token", e);
            }

            return result;
        }

        const string ExtendAccessTokenURL = "https://graph.facebook.com/oauth/access_token?client_id={0}&client_secret={1}&grant_type=fb_exchange_token&fb_exchange_token={2}";
        public static FacebookAccessToken ExchangeAccessTokenDuration(string applicationId, string secretKey, string accessToken)
        {
            FacebookAccessToken result = null;

            Uri requestUri = new Uri(string.Format(ExtendAccessTokenURL,
                applicationId,
                secretKey,
                accessToken));
            try
            {

                WebRequest request = WebRequest.Create(requestUri);
                WebResponse response = request.GetResponse();

                using (StreamReader read = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = read.ReadToEnd();
                    string[] data = responseText.Split('&');
                    if (data != null && data.Length == 2)
                    {
                        string[] token = data[0].Split('=');
                        string[] expires = data[1].Split('=');

                        if (token != null && token.Length == 2 &&
                            expires != null && expires.Length == 2)
                        {
                            int exp;
                            int.TryParse(expires[1], out exp);

                            result = new FacebookAccessToken();
                            result.AccessToken = token[1];
                            result.Expires = DateTime.UtcNow.AddSeconds(exp);

                            // handling the Facebook bug that causes the expires value to start from instead of January 1st 1970 :p
                            if (result.Expires < DateTime.UtcNow)
                            {
                                result.Expires = DateTime.UtcNow.AddSeconds(exp);
                            }
                        }
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (StreamReader read = new StreamReader(e.Response.GetResponseStream()))
                    {
                        string responseText = read.ReadToEnd();
                        read.Close();

                        Logger.Current.Error("Authentication.ExchangeAccessTokenDuration", responseText);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("Authentication.ExchangeAccessTokenDuration", e.Message, e);
            }

            return result;
        }
    }
}
