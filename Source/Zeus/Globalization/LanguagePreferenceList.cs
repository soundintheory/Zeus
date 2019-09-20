using System.Collections.Generic;
using System.Web;

namespace Zeus.Globalization
{
	public class LanguagePreferenceList : List<string>
	{
		// Methods
		public void ConditionalAdd(string languageCode)
		{
			if (!string.IsNullOrEmpty(languageCode))
			{
				var item = RemoveQualityIndicator(languageCode);
				if (!Contains(item))
				{
					Add(item);
				}
			}
		}

		public void ConditionalAddCookie(HttpCookie cookie)
		{
			if (cookie != null)
			{
				ConditionalAdd(cookie.Value);
			}
		}

		public void ConditionalAddRange(IEnumerable<string> range)
		{
			if (range != null)
			{
				foreach (var str in range)
				{
					ConditionalAdd(str);
				}
			}
		}

		public static string RemoveQualityIndicator(string browserLanguageCode)
		{
			var index = browserLanguageCode.IndexOf(';');
			if (index < 0)
			{
				return browserLanguageCode;
			}

			return browserLanguageCode.Remove(index);
		}
	}
}