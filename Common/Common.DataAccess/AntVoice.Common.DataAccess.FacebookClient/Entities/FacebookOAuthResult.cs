using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.FacebookClient.Entities
{
    public class FacebookOAuthResult<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance has error.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
        /// </value>
        public bool HasError { get; set; }
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { get; set; }

        public FacebookOAuthResult()
        {
            CredentialsHaveExpired = false;
            Data = default(T);
        }

        public void SetHasExpired()
        {
            CredentialsHaveExpired = true;
        }

        public bool CredentialsHaveExpired { get; private set; }
        public T Data { get; set; }
    }
}
