using System.Collections.Specialized;
using System.Configuration;

namespace Zeus.Configuration
{
	public class HostNameCollection : ConfigurationElementCollection
	{
		public void Add(HostNameElement hostname)
		{
			BaseAdd(hostname);
		}

		public void Clear()
		{
			BaseClear();
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new HostNameElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((HostNameElement) element).Name;
		}

		public StringDictionary GetMappings()
		{
			StringDictionary dictionary = new StringDictionary();
			foreach (HostNameElement element in this)
				if (!(element.Name == "*"))
					dictionary.Add(element.Name, element.Language);
			return dictionary;
		}
	}
}