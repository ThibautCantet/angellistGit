using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient.Entities
{
    public class FacebookAccessToken
    {
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
    }
}
