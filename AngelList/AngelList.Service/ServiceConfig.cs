using Platform.Tools.Settings;
using Platform.Tools.Settings.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.Service
{
	class ServiceConfig : ISettings
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ServiceConfig()
			: base(new AppSettingsProvider())
		{

		}

		/// <summary>
		/// Assembly name
		/// </summary>
		protected override string AssemblyName
		{
			get { return "AngelList.Service"; }
		}

		/// <summary>
		/// Frequence in hours
		/// </summary>
		public int Frequence { get { return Provider.GetConfigurationIntValue("Frequence", AssemblyName); } }
	}
}
