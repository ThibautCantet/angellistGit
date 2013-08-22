using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient.Tools
{
    public static class RetryUtility
    {
        public static void RetryActionOnException<InnerExceptionType>(int numRetries, int retryTimeout, Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            do
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException is InnerExceptionType)
                    {
                        if (numRetries <= 0)
                            throw e;
                        else
                            Thread.Sleep(retryTimeout);
                    }
                    else
                    {
                        throw e;
                    }
                }
            } while (numRetries-- > 0);
        }

        public static TResult RetryActionOnException<InnerExceptionType, TResult>(int numRetries, int retryTimeout, Func<TResult> action)
        {
            if (action == null)
                throw new ArgumentNullException("action");

            do
            {
                try
                {
                    return action();
                }
                catch (Exception e)
                {
                    if (e.InnerException != null && e.InnerException is InnerExceptionType)
                    {
                        if (numRetries <= 0)
                            throw e;
                        else
                            Thread.Sleep(retryTimeout);
                    }
                    else
                    {
                        throw e;
                    }
                }
            } while (numRetries-- > 0);

            return default(TResult);
        }
    }
}
