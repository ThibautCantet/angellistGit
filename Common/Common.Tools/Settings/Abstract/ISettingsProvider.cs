// <copyright file="ISetting.cs" company="AntVoice">
// Copyright (c)  All rights reserved.
// </copyright>

namespace Platform.Tools.Settings.Abstract
{
	using System;

	public interface ISettingsProvider
	{
		string GetConfigurationValue(string key, params string[] assemblies);
        int GetConfigurationIntValue(string key, params string[] assemblies);
        long GetConfigurationLongValue(string key, params string[] assemblies);
        bool GetConfigurationBoolValue(string key, params string[] assemblies);
	}
}
