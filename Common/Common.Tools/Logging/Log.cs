
namespace Platform.Tools.Logging
{
    using System;

    public static class Log
    {
        #region Debug

        public static void Debug(string function, string message, params object[] customProperties)
        {
            Logger.Current.Debug(function, message, customProperties);
        }
        public static void Debug(string function, string message, int userID, params object[] customProperties)
        {
            Logger.Current.Debug(function, message, userID, customProperties);
        }

        #endregion

        #region Info

        public static void Info(string function, string message, params object[] customProperties)
        {
            Logger.Current.Info(function, message, customProperties);
        }

        public static void Info(string function, string message, int userID, params object[] customProperties)
        {
            Logger.Current.Info(function, message, userID, customProperties);
        }

        #endregion

        #region Warn

        public static void Warn(string function, string message, params object[] customProperties)
        {
            Logger.Current.Warn(function, message, customProperties);
        }

        public static void Warn(string function, string message, int userID, params object[] customProperties)
        {
            Logger.Current.Warn(function, message, userID, customProperties);
        }

        #endregion

        #region Error

        public static void Error(string function, string message, params object[] customProperties)
        {
            Logger.Current.Error(function, message, customProperties);
        }

        public static void Error(string function, string message, Exception e, params object[] customProperties)
        {
            Logger.Current.Error(function, message, e, customProperties);
        }

        public static void Error(string function, string message, int userID, params object[] customProperties)
        {
            Logger.Current.Error(function, message, userID, customProperties);
        }

        public static void Error(string function, string message, int userID, Exception e, params object[] customProperties)
        {
            Logger.Current.Error(function, message, userID, e, customProperties);
        }

        #endregion
    }
}
