using System;
using System.Configuration;
using System.Globalization;
using System.Web;
using Zeus.Configuration;
using Zeus.Globalization.ContentTypes;
using Zeus.Web;

namespace Zeus.Globalization
{
	public class ContentLanguage
	{
		static ContentLanguage()
		{
			Instance = new ContentLanguage();
		}

		protected ContentLanguage()
		{
			
		}

		public static ContentLanguage Instance { get; set; }

		public CultureInfo DetermineCulture(LanguagePreferenceList preferenceList)
		{
			CultureInfo culture = null;
			foreach (var languageCode in preferenceList)
			{
                var languageManager = Context.Current.LanguageManager;
                var availableLanguages = languageManager.GetAvailableLanguages(false);
                foreach (var branch in availableLanguages)
				{
					if (string.Compare(languageCode, branch.Name, StringComparison.OrdinalIgnoreCase) == 0)
					{
						return branch.Culture;
					}

					if (culture == null && LanguageSelection.IsCandidateMatch(languageCode, branch.Name))
					{
						culture = branch.Culture;
					}
				}
				if (culture != null)
				{
					return culture;
				}
			}
			return FinalFallbackCulture;
		}

		public LanguagePreferenceList LanguagePreferenceList(string priorityLanguage)
		{
			var list = new LanguagePreferenceList();
			list.ConditionalAdd(priorityLanguage);
			if (Context.Current.WebContext.IsWeb)
			{
				var request = Context.Current.WebContext.Request;
				list.ConditionalAdd(request.QueryString["zeuslanguage"]);
				if (HttpRequestSupport.IsRequestSystemDirectory)
				{
					list.ConditionalAddCookie(request.Cookies["editlanguagebranch"]);
				}

				list.ConditionalAdd(Context.Current.Resolve<IHost>().GetLanguageFromHostName());
				list.ConditionalAddCookie(request.Cookies["zeuslanguage"]);

				var globalizationConfig = ConfigurationManager.GetSection("zeus/globalization") as GlobalizationSection;
				if (globalizationConfig != null && globalizationConfig.UseBrowserLanguagePreferences)
				{
					list.ConditionalAddRange(request.UserLanguages);
				}

				list.ConditionalAdd(CultureInfo.CurrentUICulture.Name);
			}
			return list;
		}

		public CultureInfo SetCulture(string priorityLanguage)
		{
			var info = DetermineCulture(LanguagePreferenceList(priorityLanguage));
			PreferredCulture = info;
			return info;
		}

		public CultureInfo SetCulture()
		{
			return SetCulture(null);
		}

		// Properties
		public CultureInfo FinalFallbackCulture
		{
			get { return Context.Current.LanguageManager.GetLanguage(Context.Current.LanguageManager.GetDefaultLanguage()).Culture; }
		}

		public static CultureInfo PreferredCulture
		{
			get
			{
				var info = Context.Current.WebContext.RequestItems["Zeus:ContentLanguage"] as CultureInfo;
				if (info == null)
				{
					info = Instance.SetCulture();
				}

				return info;
			}
			set
			{
				Context.Current.WebContext.RequestItems["Zeus:ContentLanguage"] = value;
			}
		}
	}
}