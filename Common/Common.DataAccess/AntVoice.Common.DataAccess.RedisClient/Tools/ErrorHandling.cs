using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient.Tools
{
    public static class ErrorHandling
    {
        public static void ThrowTimeoutException(string cache, string key, TimeoutException exception)
        {
            throw new TimeoutException("A timeout occured while trying to access a key (" + key + ") in " + cache + " - " + exception.Message, exception);
        }

        public static void ThrowKeyDoesNotExistException(string cache, string key, KeyNotFoundException exception)
        {
            throw new KeyNotFoundException("Key (" + key + ") was not found in " + cache + " - " + exception.Message, exception);
        }

        public static void ThrowGeneralException(string cache, string key, Exception exception)
        {
            throw new Exception("General exception thrown for key " + key + " in " + cache + " - " + exception.Message, exception);
        }
    }
}
