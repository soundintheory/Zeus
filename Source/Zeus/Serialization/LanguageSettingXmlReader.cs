using System.Collections.Generic;
using System.Xml.XPath;
using Zeus.Globalization;
using Zeus.Security;

namespace Zeus.Serialization
{
	public class LanguageSettingXmlReader : XmlReader, IXmlReader
	{
		public void Read(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			foreach (var languageSettingElement in EnumerateChildren(navigator))
			{
				var attributes = GetAttributes(languageSettingElement);
				var language = attributes["language"];
				var fallbackLanguage = attributes["fallbackLanguage"];
				item.LanguageSettings.Add(new LanguageSetting(item, language, fallbackLanguage));
			}
		}
	}
}