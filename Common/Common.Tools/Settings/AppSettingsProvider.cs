// <copyright file="Settings.cs" company="AntVoice">
// Copyright (c)  All rights reserved.
// </copyright>

namespace Platform.Tools.Settings
{
    using Platform.Tools.Settings.Abstract;
    using System;
    using System.Configuration;

	/// <summary>
	/// A library to acces 
	/// </summary>
	public class AppSettingsProvider : ISettingsProvider
	{
		public string GetConfigurationValue(string key, params string[] assemblies)
		{
			if (ConfigurationManager.AppSettings[key] == null)
			{
				throw new ConfigurationErrorsException("You have to add the " + key + " key to your appSettings in order for " + string.Join(",", assemblies) + " to work");
			}
			return ConfigurationManager.AppSettings[key];
		}

        public int GetConfigurationIntValue(string key, params string[] assemblies)
        {
            int config = 0;
            if (!int.TryParse(GetConfigurationValue(key, assemblies), out config))
            {
                throw new ConfigurationErrorsException("The configuration " + key + " in your appSettings for " + string.Join(",", assemblies) + " must be an integer");
            }

            return config;
        }

        public long GetConfigurationLongValue(string key, params string[] assemblies)
		{
			long config = 0;
			if (!long.TryParse(GetConfigurationValue(key, assemblies), out config))
			{
				throw new ConfigurationErrorsException("The configuration " + key + " in your appSettings for " + string.Join(",", assemblies) + " must be a long");
			}

			return config;
		}

        public bool GetConfigurationBoolValue(string key, params string[] assemblies)
        {
            bool config = false;
            if (!bool.TryParse(GetConfigurationValue(key, assemblies), out config))
            {
                throw new ConfigurationErrorsException("The configuration " + key + " in your appSettings for " + string.Join(",", assemblies) + " must be a bool");
            }

            return config;
        }
	}
}
