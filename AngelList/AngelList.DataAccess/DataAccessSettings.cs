using Platform.Tools.Settings;
using Platform.Tools.Settings.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngelList.DataAccess
{
	internal class DataAccessSettings : ISettings
	{
		public DataAccessSettings() : base(new AppSettingsProvider()) { }

		protected override string AssemblyName
		{
			get { return "AngelList.DataAccess"; }
		}

		public string MongoDBConnectionString { get { return Provider.GetConfigurationValue("MongoDBConnectionString", AssemblyName); } }

	}
}
