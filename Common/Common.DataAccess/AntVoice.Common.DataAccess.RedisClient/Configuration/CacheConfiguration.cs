using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntVoice.Common.DataAccess.RedisClient.Configuration
{
    public class CacheConfiguration : ConfigurationSection
    {
        private const string SectionName = "cacheConfig";

        public static CacheConfiguration Current
        {
            get { return (CacheConfiguration)ConfigurationManager.GetSection(SectionName); }
        }

        #region <serviceConfiguration />

        private const string RedisProperty = "redis";

        [ConfigurationProperty(RedisProperty)]
        public RedisSection Redis
        {
            get { return (RedisSection)base[RedisProperty]; }
        }

        #endregion

        public static CacheConfiguration GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("<cacheConfig />");
            }

            return (CacheConfiguration)config.Sections[SectionName];
        }
    }

    public sealed class RedisSection : ConfigurationElement
    {
        private const string SectionName = "redis";

        public static CacheConfiguration GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("<redis />");
            }

            return (CacheConfiguration)config.Sections[SectionName];
        }

        private const string HostsProperty = "hosts";

        [ConfigurationProperty(HostsProperty)]
        public HostsSection Hosts
        {
            get { return (HostsSection)base[HostsProperty]; }
        }

        private const string DictionariesProperty = "dictionaries";

        [ConfigurationProperty(DictionariesProperty)]
        public DictionariesSection Dictionaries
        {
            get { return (DictionariesSection)base[DictionariesProperty]; }
        }

        #region MULTI_ENV Environment variable

        public string MultiEnvironment
        {
            get
            {
                try
                {
                    return System.Environment.GetEnvironmentVariable("MULTI_ENV", EnvironmentVariableTarget.Machine).Replace(" ", "");
                }
                catch (Exception) { return string.Empty; }
            }
        }

        #endregion
    }

    public sealed class HostsSection : ConfigurationElementCollection
    {
        private const string SectionName = "host";

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        protected override string ElementName
        {
            get { return SectionName; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return new ConfigurationPropertyCollection(); }
        }

        public HostSettings this[int index]
        {
            get { return (HostSettings)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new HostSettings this[string name]
        {
            get { return (HostSettings)BaseGet(name); }
        }

        public void Add(HostSettings item)
        {
            base.BaseAdd(item);
        }

        public void Remove(HostSettings item)
        {
            BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Clear()
        {
            BaseClear();
        }

        public List<HostSettings> GetAllKeys()
        {
            object[] list = BaseGetAllKeys();
            List<HostSettings> result = new List<HostSettings>();
            foreach (string key in list)
            {
                result.Add(CacheConfiguration.Current.Redis.Hosts[key]);
            }
            return result;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HostSettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element != null)
                return ((HostSettings)element).Name;
            else
                return null;
        }
    }

    public sealed class DictionariesSection : ConfigurationElementCollection
    {
        private const string SectionName = "dictionary";

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
        protected override string ElementName
        {
            get { return SectionName; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return new ConfigurationPropertyCollection(); }
        }

        public DictionarySettings this[int index]
        {
            get { return (DictionarySettings)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new DictionarySettings this[string name]
        {
            get { return (DictionarySettings)BaseGet(name); }
        }

        public void Add(DictionarySettings item)
        {
            base.BaseAdd(item);
        }

        public void Remove(DictionarySettings item)
        {
            BaseRemove(item);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Clear()
        {
            BaseClear();
        }

        public List<DictionarySettings> GetAllKeys()
        {
            object[] list = BaseGetAllKeys();
            List<DictionarySettings> result = new List<DictionarySettings>();
            foreach (string key in list)
            {
                result.Add(CacheConfiguration.Current.Redis.Dictionaries[key]);
            }
            return result;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DictionarySettings();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element != null)
                return ((DictionarySettings)element).Name;
            else
                return null;
        }
    }

    public sealed class HostSettings : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _name;
        private static readonly ConfigurationProperty _url;
        private static readonly ConfigurationProperty _port;
        private static readonly ConfigurationProperty _maxclients;

        static HostSettings()
        {
            _name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey);
            _url = new ConfigurationProperty("url", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _port = new ConfigurationProperty("port", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _maxclients = new ConfigurationProperty("maxClients", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_name);
            _properties.Add(_url);
            _properties.Add(_port);
            _properties.Add(_maxclients);
        }

        #region name=""

        private const string NameProperty = "name";

        [ConfigurationProperty(NameProperty)]
        public string Name
        {
            get { return (string)base[NameProperty]; }
            set { base[NameProperty] = value; }
        }

        #endregion

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

        #region maxclients=""

        private const string MaxClientsProperty = "maxClients";

        [ConfigurationProperty(MaxClientsProperty)]
        public int MaxClients
        {
            get { return int.Parse(base[MaxClientsProperty].ToString()); }
            set { base[MaxClientsProperty] = value; }
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }

    public sealed class DictionarySettings : ConfigurationElement
    {
        private static readonly ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _name;
        private static readonly ConfigurationProperty _host;

        static DictionarySettings()
        {
            _name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsKey);
            _host = new ConfigurationProperty("host", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_name);
            _properties.Add(_host);
        }

        #region name=""

        private const string NameProperty = "name";

        [ConfigurationProperty(NameProperty)]
        public string Name
        {
            get { return (string)base[NameProperty]; }
            set { base[NameProperty] = value; }
        }

        #endregion

        #region host=""

        private const string HostProperty = "host";

        [ConfigurationProperty(HostProperty)]
        public string Host
        {
            get { return (string)base[HostProperty]; }
            set { base[HostProperty] = value; }
        }

        #endregion

        protected override ConfigurationPropertyCollection Properties
        {
            get { return _properties; }
        }
    }
}
