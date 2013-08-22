using Platform.Tools.Logging.Abstract;
using Platform.Tools.Settings;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Platform.Tools.Logging
{
    public class Logger : ILogger
    {
        private ToolsAppSettings _settings = new ToolsAppSettings();

        #region Singleton

        private Logger()
        {
            ConfigurationManager.RefreshSection("log4net");

            log4net.Config.XmlConfigurator.Configure();
            log4net.GlobalContext.Properties["component"] = _settings.ComponentName;

            log = LogManager.GetLogger("LogFileAppender");
        }

        private static readonly Logger _instance = new Logger();

        public static Logger Current { get { return _instance; } }

        #endregion

        #region Properties

        private static ILog log = LogManager.GetLogger("LogFileAppender");

        private const string LOG_FORMAT = "[{0,-60}]\t[{1,-11}]\t{2}\t[{3}]";

        public bool IsDebugEnabled { get { return log.IsDebugEnabled; } }

        #endregion

        #region Debug

        public void Debug(string function, string message, params object[] customProperties)
        {
            Debug(function, message, 0, customProperties);
        }
        public void Debug(string function, string message, int userID, params object[] customProperties)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(GenerateLog(function, message, userID, customProperties));
            }
        }

        #endregion

        #region Info

        public void Info(string function, string message, params object[] customProperties)
        {
            Info(function, message, 0, customProperties);
        }
        public void Info(string function, string message, int userID, params object[] customProperties)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(GenerateLog(function, message, userID, customProperties));
            }
        }

        #endregion

        #region Warn

        public void Warn(string function, string message, params object[] customProperties)
        {
            Warn(function, message, 0, customProperties);
        }
        public void Warn(string function, string message, int userID, params object[] customProperties)
        {
            if (log.IsWarnEnabled)
            {
                log.Warn(GenerateLog(function, message, userID, customProperties));
            }
        }

        #endregion

        #region Error

        public void Error(string function, string message, params object[] customProperties)
        {
            Error(function, message, 0, null, customProperties);
        }
        public void Error(string function, string message, Exception e, params object[] customProperties)
        {
            Error(function, message, 0, e, customProperties);
        }
        public void Error(string function, string message, int userID, params object[] customProperties)
        {
            Error(function, message, userID, null, customProperties);
        }
        public void Error(string function, string message, int userID, Exception e, params object[] customProperties)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(GenerateLog(function, message, userID, customProperties), e);
            }
        }

        #endregion

        #region Utils

        private string GenerateLog(string function, string message, int userID, params object[] customProperties)
        {
            StringBuilder customPropertiesSb = null;
            if (customProperties != null && customProperties.Length > 0)
            {
                customPropertiesSb = new StringBuilder();
                for (int i = 0; i < customProperties.Length; i++)
                {
                    if (customProperties[i] != null)
                    {
                        if (i < customProperties.Length - 1)
                        {
                            customPropertiesSb.AppendFormat("{0};", customProperties[i].ToString());
                        }
                        else
                        {
                            customPropertiesSb.Append(customProperties[i].ToString());
                        }
                    }
                }
            }

            return string.Format(
                LOG_FORMAT,
                string.IsNullOrEmpty(function) ? "-" : function.Replace('\t', ' '),	// current function name
                userID,																// the current userID if applicable
                string.IsNullOrEmpty(message) ? "-" : message.Replace('\t', ' '),	// message to log
                customPropertiesSb != null ? customPropertiesSb.ToString() : "-"	// custom properties
            );
        }

        #endregion
    }
}
