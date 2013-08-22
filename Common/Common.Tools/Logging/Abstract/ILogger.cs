using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Logging.Abstract
{
    public interface ILogger
    {
        void Debug(string function, string message, params object[] customProperties);
        void Debug(string function, string message, int userID, params object[] customProperties);
        void Error(string function, string message, params object[] customProperties);
        void Error(string function, string message, Exception e, params object[] customProperties);
        void Error(string function, string message, int userID, params object[] customProperties);
        void Error(string function, string message, int userID, Exception e, params object[] customProperties);
        void Info(string function, string message, params object[] customProperties);
        void Info(string function, string message, int userID, params object[] customProperties);
        bool IsDebugEnabled { get; }
        void Warn(string function, string message, params object[] customProperties);
        void Warn(string function, string message, int userID, params object[] customProperties);
    }
}
