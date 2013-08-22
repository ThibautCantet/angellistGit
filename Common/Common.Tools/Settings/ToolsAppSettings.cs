using Platform.Tools.Settings.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Settings
{
    internal class ToolsAppSettings : ISettings
    {
        internal ToolsAppSettings() : base(new AppSettingsProvider()) { }

        protected override string AssemblyName
        {
            get { return "Platform.Tools"; }
        }

        public string ComponentName { get { return Provider.GetConfigurationValue("ComponentName", AssemblyName); } }
    }
}
