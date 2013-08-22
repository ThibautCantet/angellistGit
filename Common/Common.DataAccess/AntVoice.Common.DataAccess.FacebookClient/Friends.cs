using AntVoice.Common.DataAccess.FacebookClient.Entities;
using AntVoice.Common.DataAccess.FacebookClient.Exceptions;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    public sealed class Friends
    {

        public static FacebookOAuthResult<FacebookFriends> GetUserFriends(string facebookId, string accessToken)
        {
            FacebookOAuthResult<FacebookFriends> result = new FacebookOAuthResult<FacebookFriends>();
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            result.Data = new FacebookFriends();
            try
            {
                result.Data.Next = facebookId + "/friends?format=json&limit=5000";
                do
                {
                    dynamic d = GraphAPI.Get(result.Data.Next, accessToken);
                    FacebookFriends pageResult = ConvertToFacebookSocialGraph(d);

                    if (pageResult.Friends != null && pageResult.Friends.Count > 0)
                    {
                        result.Data.Friends.AddRange(pageResult.Friends);
                        result.Data.Next = pageResult.Next;

                        if (pageResult.Friends.Count < 5000)
                        {
                            result.Data.Next = null;
                        }
                    }
                    else
                    {
                        result.Data.Next = null;
                    }
                }
                while (!string.IsNullOrEmpty(result.Data.Next));
            }
            catch (FacebookOAuthException)
            {
                result.SetHasExpired();
            }
            catch (Exception e)
            {
                Logger.Current.Error("SocialGraph.GetFriendIds", "Could not update a Facebook user's social graph", e);
                result.HasError = true;
            }
            finally
            {
                //TODO : ajouter un compteur sur le nombre d'amis récupérés
                watch.Stop();
                MonitoringTimers.Current.AddTime(Counters.Facebook_GetUserFriends, watch);
            }

            return result;
        }

        private static FacebookFriends ConvertToFacebookSocialGraph(dynamic d)
        {
            FacebookFriends socialGraph = new FacebookFriends();

            try
            {
                if (d.data != null)
                {
                    for (int i = 0; i < d.data.Count; i++)
                    {
                         string id = d.data[i].id.ToString() ?? string.Empty;
                         socialGraph.Friends.Add(id);
                    }
                }

                if (d.paging != null && d.paging.next != null)
                {
                    socialGraph.Next = d.paging.next.ToString();
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("SocialGraph.ConvertToFacebookSocialGraph", "Could not convert dynamic object to FacebookSocialGraph", e);
            }

            return socialGraph;
        }
    }
}
