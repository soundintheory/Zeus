using System.Xml;
using Zeus.Globalization;

namespace Zeus.Serialization
{
	public class LanguageSettingXmlWriter : IXmlWriter
	{
		public virtual void Write(ContentItem item, XmlTextWriter writer)
		{
			using (new ElementWriter("languageSettings", writer))
			{
				if (item.LanguageSettings != null)
					foreach (var ar in item.LanguageSettings)
						WriteRule(writer, ar);
			}
		}

		protected virtual void WriteRule(XmlTextWriter writer, LanguageSetting ls)
		{
			using (var role = new ElementWriter("setting", writer))
			{
				role.WriteAttribute("language", ls.Language);
				role.WriteAttribute("fallbackLanguage", ls.FallbackLanguage);
			}
		}
	}
}