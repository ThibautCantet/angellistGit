using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Settings.Abstract
{
    public abstract class ISettings
    {
        private ISettingsProvider _provider;

        public ISettings(ISettingsProvider provider)
        {
            _provider = provider;
        }

        protected abstract string AssemblyName { get; }
        protected ISettingsProvider Provider { get { return _provider; } }
    }
}
