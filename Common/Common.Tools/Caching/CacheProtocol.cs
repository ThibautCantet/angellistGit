using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Caching
{
    #region Cache related tools

    /// <summary>
    /// Defines a factory to generate unique cache keys based on key uri mecanism
    /// </summary>
    internal static class CacheProtocol
    {
        private static readonly string protocol = "cp://";
        private static readonly string modelUrl = "{0}{1}/{2}";		// 0=cache,1=main key,2=parameters
        private static readonly string modelKey = "/";              // 0=key

        /// <summary>
        /// build the cache uri key
        /// </summary>
        /// <param name="_mainKey">the main cache key</param>
        /// <param name="args">optional parameters for main key</param>
        /// <returns>string uri</returns>
        public static string GetURI(string _mainKey, params string[] args)
        {
            string keys = string.Join(modelKey, args);
            return string.Format(modelUrl, protocol, _mainKey.ToString(), keys);
        }

        public static string GetURI()
        {
            return GetURI(string.Empty);
        }
    }

    #endregion
}
