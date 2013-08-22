using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient.Exceptions
{
    public class FacebookApiException : Exception
    {
        public FacebookApiException(string type, string content)
        {
            Type = type;
            Content = content;
        }

        public string Type { get; set; }
        public string Content { get; set; }
    }

    public class FacebookOAuthException : FacebookApiException
    {
        public FacebookOAuthException(string type, string content)
            : base(type, content)
        {
        }
    }
}
