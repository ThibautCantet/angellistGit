using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AntVoice.Common.DataAccess.FacebookClient.Entities
{
    [Serializable]
    [XmlRoot("sc")]
    public class FacebookFriends
    {
        public FacebookFriends()
		{
			Friends = new List<string>();
		}

		[XmlArray("flst")]
		[XmlArrayItem(("f"), typeof(string))]
		public List<string> Friends { get; set; }

		[XmlIgnore]
		public string Next { get; set; }
    }
}
