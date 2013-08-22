using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.NeoClient.Configuration
{
    public class GraphConfiguration : ConfigurationSection
    {
        private const string SectionName = "graphConfig";

        public static GraphConfiguration Current
        {
            get { return (GraphConfiguration)ConfigurationManager.GetSection(SectionName); }
        }

        #region timeout=""

        private const string TimeoutProperty = "timeout";

        [ConfigurationProperty(TimeoutProperty)]
        public int Timeout
        {
            get { return (int)base[TimeoutProperty]; }
            set { base[TimeoutProperty] = value; }
        }

        #endregion

        #region <reader />

        private const string ReaderProperty = "reader";

        [ConfigurationProperty(ReaderProperty)]
        public ReaderSettings Reader
        {
            get { return (ReaderSettings)base[ReaderProperty]; }
        }

        #endregion

        #region <writer />

        private const string WriterProperty = "writer";

        [ConfigurationProperty(WriterProperty)]
        public WriterSettings Writer
        {
            get { return (WriterSettings)base[WriterProperty]; }
        }

        #endregion

        public static GraphConfiguration GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("<graphConfig />");
            }

            return (GraphConfiguration)config.Sections[SectionName];
        }
    }

    public sealed class ReaderSettings : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _url;
        private static readonly ConfigurationProperty _port;

        static ReaderSettings()
        {
            _url = new ConfigurationProperty("url", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _port = new ConfigurationProperty("port", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_url);
            _properties.Add(_port);
        }

        #region url=""

        private const string URLProperty = "url";

        [ConfigurationProperty(URLProperty)]
        public string URL
        {
            get { return (string)base[URLProperty]; }
            set { base[URLProperty] = value; }
        }

        #endregion

        #region port=""

        private const string PortProperty = "port";

        [ConfigurationProperty(PortProperty)]
        public int Port
        {
            get { return int.Parse(base[PortProperty].ToString()); }
            set { base[PortProperty] = value; }
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public override string ToString()
        {
            return string.Concat(URL, ":", Port);
        }
    }

    public sealed class WriterSettings : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _url;
        private static readonly ConfigurationProperty _port;

        static WriterSettings()
        {
            _url = new ConfigurationProperty("url", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _port = new ConfigurationProperty("port", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_url);
            _properties.Add(_port);
        }

        #region url=""

        private const string URLProperty = "url";

        [ConfigurationProperty(URLProperty)]
        public string URL
        {
            get { return (string)base[URLProperty]; }
            set { base[URLProperty] = value; }
        }

        #endregion

        #region port=""

        private const string PortProperty = "port";

        [ConfigurationProperty(PortProperty)]
        public int Port
        {
            get { return int.Parse(base[PortProperty].ToString()); }
            set { base[PortProperty] = value; }
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }

        public override string ToString()
        {
            return string.Concat(URL, ":", Port);
        }
    }
}
