using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient
{
    public static class WebResponseExtensions
    {
        public static string GetBody(this WebResponse response)
        {
            if (response != null && response.ContentLength > 0)
            {
                Stream s = response.GetResponseStream();
                if (s != null)
                {
                    string result = new StreamReader(s).ReadToEnd();
                    s.Seek(0, SeekOrigin.Begin);

                    return result;
                }
            }

            return string.Empty;
        }
    }
}
