using Platform.Tools.Settings;
using Platform.Tools.Settings.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Crm
{
	internal class CrmSettings : ISettings
	{
		public CrmSettings() : base(new AppSettingsProvider()) { }

		protected override string AssemblyName
		{
			get { return "AngelList.Crm"; }
		}

		public string ThibautEmail { get { return Provider.GetConfigurationValue("ThibautEmail", AssemblyName); } }
		public string ClementEmail { get { return Provider.GetConfigurationValue("ClementEmail", AssemblyName); } }
		public string Thibaut { get { return Provider.GetConfigurationValue("Thibaut", AssemblyName); } }
		public string Clement { get { return Provider.GetConfigurationValue("Clement", AssemblyName); } }
	}
}
