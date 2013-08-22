using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Tools.Monitoring.Protocol
{
    internal abstract class TrapperItem
	{
		private const string JSON_PATTERN =
			"{{" +
			"\"host\":\"{0}\"," +
			"\"key\":\"{1}_{2}_{3}\"," +
			"\"value\":\"{4}\"" +
			"}}"
			;

		public string Key { get; private set; }
		public string Value { get; private set; }
		protected abstract string Prefix { get; }

		public TrapperItem(string key, string value)
		{
			Key = key;
			Value = value;
		}

		public string ToJson(string host, string componentName)
		{
			return string.Format(JSON_PATTERN, host, componentName, Prefix, Key, Value);
		}
	}
}
