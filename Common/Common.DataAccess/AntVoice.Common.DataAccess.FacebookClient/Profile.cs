using AntVoice.Common.DataAccess.FacebookClient.Entities;
using AntVoice.Common.DataAccess.FacebookClient.Exceptions;
using AntVoice.Common.Entities.Users.Facebook;
using AntVoice.Platform.Tools.Logging;
using AntVoice.Platform.Tools.Monitoring;
using AntVoice.Platform.Tools.Monitoring.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    public sealed class Profile
    {
        public static FacebookOAuthResult<FacebookUser> GetUserProfileSync(FacebookSignedRequest signedRequest, string fields)
        {
			if (signedRequest == null)
			{
				Logger.Current.Error("GetUserProfileSync", "The Facebook signed request is null. Cannot request the facebook profile");
				return null;
			}

            FacebookOAuthResult<FacebookUser> result = new FacebookOAuthResult<FacebookUser>();

            if (string.IsNullOrEmpty(fields))
            {
                fields = "likes.offset(0).limit(1000),id,name,first_name,last_name,gender,locale,picture,hometown,link,birthday,location,meeting_for,email,timezone,relationship_status,significant_other";
            }

            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                dynamic d = GetUserProfileFieldSync(signedRequest.UserID, fields, signedRequest.AccessToken);

                if ((bool)d.CredentialsHaveExpired)
                {
                    result.SetHasExpired();
                }
                result.Data = ConvertToFacebookUser(d);
            }
            catch (FacebookOAuthException)
            {
                result.SetHasExpired();
            }
            catch (Exception e)
            {
                Logger.Current.Error("Profile.GetUserProfileSync", "Could not retrieve Facebook user's profile information", e);
            }
            finally
            {
                watch.Stop();
                var counter = Counters.Facebook_GetUserProfile;
                if (fields.Contains("likes"))
                {
                    counter = Counters.Facebook_GetUserLikesAndProfile;
                }
                MonitoringTimers.Current.AddTime(counter, watch);
            }

            return result;
        }

        public static FacebookOAuthResult<dynamic> GetUserProfileFieldSync(string facebookId, string fields, string accessToken)
        {
            FacebookOAuthResult<dynamic> result = new FacebookOAuthResult<dynamic>();

            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                dynamic d = GraphAPI.Get(
                    string.Format("{0}?fields={1}",
                        facebookId,
                        fields), accessToken);

                result.Data = d;
            }
            catch (FacebookOAuthException)
            {
                result.SetHasExpired();
            }
            catch (Exception e)
            {
                Logger.Current.Error("Profile.GetUserProfileFieldSync", "Could not retrieve Facebook user's profile field", e, fields);
            }
            finally
            {
                watch.Stop();
                var counter = Counters.Facebook_GetUserProfile;
                if (fields.Contains("likes"))
                {
                    counter = Counters.Facebook_GetUserLikesAndProfile;
                }
                MonitoringTimers.Current.AddTime(counter, watch);
            }

            return result;
        }

        public static FacebookOAuthResult<FacebookLikes> GetAllUserLikes(string facebookId, string accessToken)
        {
            FacebookOAuthResult<FacebookLikes> result = new FacebookOAuthResult<FacebookLikes>();
            result.Data = new FacebookLikes();
            IStopwatch watch = MonitoringTimers.Current.GetNewStopwatch(true);
            try
            {
                result.Data.Next = facebookId + "/likes?format=json&limit=5000";
                do
                {

                    dynamic d = GraphAPI.Get(result.Data.Next, accessToken);

                    FacebookLikes pageResult = ConvertToFacebookLikes(d);

                    if (pageResult.Likes != null && pageResult.Likes.Count > 0)
                    {
                        result.Data.Likes.AddRange(pageResult.Likes);
                        result.Data.Next = pageResult.Next;

                        if (pageResult.Likes.Count < 5000)
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
                result.HasError = true;
                Logger.Current.Error("SocialGraph.GetFriendIds", "Could not update a Facebook user's social graph", e);
            }
            finally
            {
                //TODO : ajouter un compteur sur le nombre de like récupérés
                    watch.Stop();
                    MonitoringTimers.Current.AddTime(Counters.Facebook_GetUserLikes, watch);
            }

            return result;
        }

        private static FacebookUser ConvertToFacebookUser(dynamic data)
        {
            FacebookUser user = new FacebookUser();
            var d = data.Data;

            if (d == null)
            {
                return user;
            }

            try
            {
                user.ID = d.id != null ? d.id.ToString() : string.Empty;
                user.Name = d.name != null ? d.name.ToString() : string.Empty;
                user.FirstName = d.first_name != null ? d.first_name.ToString() : string.Empty;
                user.LastName = d.last_name != null ? d.last_name.ToString() : string.Empty;

                if (d.picture != null && d.picture.data != null)
                {
                    user.SquarePicture = d.picture.data.url;
                }

                if (d.gender != null)
                {
                    if (d.gender.ToString() == "male")
                    {
                        user.Gender = FacebookUserSex.Male;
                    }
                    else if (d.gender.ToString() == "female")
                    {
                        user.Gender = FacebookUserSex.Female;
                    }
                }

                user.Locale = d.locale != null ? d.locale : string.Empty;
                user.Country = string.IsNullOrEmpty(user.Locale) ? string.Empty : user.Locale.Split('_')[1];

                user.Link = d.link != null ? d.link : string.Empty;

                user.Email = d.email != null ? d.email : string.Empty;

                DateTime date = DateTime.MinValue;
                if (d.birthday != null &&
                    DateTime.TryParse(d.birthday.ToString(), new CultureInfo("en-US"), DateTimeStyles.None, out date))
                {
                    user.DateOfBirth = date;
                }
                else
                {
                    user.DateOfBirth = null;
                }

                if (d.location != null && d.location.name != null)
                {
                    user.Location = d.location.name.ToString();
                }
                else
                {
                    user.Location = string.Empty;
                }

				user.MeetingFor = d.meeting_for != null ? d.meeting_for.ToString() : string.Empty;
				user.RelationshipStatus = d.relationship_status != null ? d.relationship_status.ToString() : string.Empty;
				user.SignificantOther = d.significant_other != null ? d.significant_other.ToString() : string.Empty;

                /*int timezone;
                if (int.TryParse(d.timezone != null ? d.timezone.ToString() : 0, out timezone))
                {
                    user.TimeZone = timezone;
                }*/

                //Retrieve likes
                if (d.likes != null && d.likes.data != null)
                {
                    for (var i = 0; i < d.likes.data.Count; i++)
                    {
                        user.FacebookLikes.Likes.Add(new Like()
                        {
                            Category = d.likes.data[i].category.ToString() ?? string.Empty,
                            Id = d.likes.data[i].id.ToString() ?? string.Empty,
                            Label = d.likes.data[i].name.ToString() ?? string.Empty
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("Profile.ConvertToFacebookUser", "Could not convert dynamic object to FacebookUser", e);
            }

            return user;
        }

        private static FacebookLikes ConvertToFacebookLikes(dynamic d)
        {
            FacebookLikes flike = new FacebookLikes();
            try
            {
                if (d.data != null)
                {
                    for (int i = 0; i < d.data.Count; i++)
                    {
                            string id = d.data[i].id.ToString() ?? string.Empty;

                            flike.Likes.Add(new Like()
                            {
                                Category = d.data[i].category.ToString() ?? string.Empty,
                                Id = d.data[i].id.ToString() ?? string.Empty,
                                Label = d.data[i].name.ToString() ?? string.Empty
                            });
                    }
                }

                if (d.paging != null && d.paging.next != null)
                {
                    flike.Next = d.paging.next.ToString();
                }
            }
            catch (Exception e)
            {
                Logger.Current.Error("SocialGraph.ConvertToFacebookSocialGraph", "Could not convert dynamic object to FacebookLikes", e);
            }

            return flike;
        }

    }
}
