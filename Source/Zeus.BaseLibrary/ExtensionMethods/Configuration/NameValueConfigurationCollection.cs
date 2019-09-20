using System.Collections.Specialized;
using System.Configuration;

namespace Zeus.BaseLibrary.ExtensionMethods.Configuration
{
	public static class NameValueConfigurationCollectionExtensionMethods
	{
		public static NameValueCollection ToNameValueCollection(this NameValueConfigurationCollection collection)
		{
			var result = new NameValueCollection();
			foreach (string key in collection)
				result.Add(key, collection[key].Value);
			return result;
		}
	}
}