using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Configuration;
using Platform.Tools.Settings;
using Platform.Tools.Monitoring.Protocol;
using Platform.Tools.Logging;

namespace Platform.Tools.Monitoring
{
    internal class ZabbixConfig
    {
		private ToolsAppSettings _config = new ToolsAppSettings();

        /// <summary>
        /// We use this filepath to get the name of the machine instance
        /// In our environments, all our servers have zabbix agent installed in this directory
        /// If we don't find this file, we use the machine name
        /// </summary>
        private const string Zabbix_Config_Filename = @"c:\zabbix\conf\zabbix_agentd.conf";

        #region Singleton

        private static readonly ZabbixConfig _instance = new ZabbixConfig();

        private ZabbixConfig() { }

        public static ZabbixConfig Current { get { return _instance; } }

        #endregion

        public string Host { get; private set; }
        public string ServerIP { get; private set; }
        public int ServerPort { get; private set; }
        public string ComponentName { get; private set; }

        public void Initialize()
        {
            try
            {
                try
                {
                    IniFile config = new IniFile(Zabbix_Config_Filename);

                    ServerIP = config.IniReadValue(string.Empty, "Server");
                    Host = config.IniReadValue(string.Empty, "Hostname");
                    try
                    {
                        ServerPort = int.Parse(config.IniReadValue(string.Empty, "ServerPort"));
                    }
                    catch (Exception)
                    {
                        // ServerPort can be absent from the config file - Setting the default port
                        ServerPort = 10051;
                    }
                }
                catch (FileNotFoundException fe)
                {
                    Logger.Current.Error("ZabbixInitialize", "Couldn't find Zabbix Config File - Using default value instead", fe);
                    ServerIP = "monitoring.com";
                    Host = Environment.MachineName;
                    ServerPort = 10051;
                }
                catch (Exception e)
                {
                    Logger.Current.Error("ZabbixInitialize", "Couldn't initialize Zabbix configuration - Zabbix sending is disabled", e);
                }

                ComponentName = _config.ComponentName;

                ZabbixSender.StartProcess();

                Logger.Current.Info("Zabbix.Initialize", "Started Zabbix Monitoring", ComponentName, ServerIP, ServerPort, Host);
            }
            catch (Exception e)
            {
                Logger.Current.Error("Zabbix.Initialize", "Zabbix initialization failed", e);
            }
        }
    }

    public class IniFile
    {
        public string path;

        private Dictionary<string, string> _properties = new Dictionary<string, string>();
        /// <summary>
        /// INIFile Constructor.
        /// </summary>
        /// <PARAM name="INIPath"></PARAM>
        public IniFile(string INIPath)
        {
            path = INIPath;
            LoadFile();
        }

        private void LoadFile()
        {
            using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                byte[] content = new byte[fs.Length];
                fs.Read(content, 0, Convert.ToInt32(fs.Length));
                string strContent = Encoding.ASCII.GetString(content);
                StringReader reader = new StringReader(strContent);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimed = line.Trim();
                    if (!string.IsNullOrEmpty(trimed) && !trimed.StartsWith("#"))
                    {
                        string[] values = trimed.Split('=');
                        if (values.Length == 2)
                        {
                            _properties[values[0]] = values[1];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            if (_properties.ContainsKey(Key))
                return _properties[Key];
            return string.Empty;
        }
    }
}
