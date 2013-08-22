using AntVoice.Platform.Tools.Settings;
using AntVoice.Platform.Tools.Settings.Abstract;

namespace AntVoice.Common.DataAccess
{
    internal class DataAccessSettings : ISettings
    {
        public DataAccessSettings() : base(new AppSettingsProvider()) { }

        protected override string AssemblyName
        {
            get { return "AntVoice.Common.DataAccess"; }
        }

        public string MongoDBRecommendationConnectionString { get { return Provider.GetConfigurationValue("MongoDBRecommendationConnectionString", AssemblyName); } }
    }
}
