using System.Globalization;
using Zeus.Globalization.ContentTypes;

namespace Zeus.Globalization
{
	public class LanguageSelectorContext
	{
		private string _selectedLanguage;
		private Language _selectedLanguageBranch;

		// Methods
		public LanguageSelectorContext(ContentItem page)
		{
			Page = page;
		}

		public bool IsLanguagePublished(string language)
		{
			if (Page == null)
			{
				throw new ZeusException("IsLanguagePublished is only supported in SelectPageLanguage where a page must exist");
			}

			var pageLanguage = Context.Current.LanguageManager.GetTranslationDirect(Page, language);
			if (pageLanguage == null)
			{
				return false;
			}

			return pageLanguage.IsPublished();
		}

		// Properties
		public string MasterLanguageBranch
		{
			get
			{
				if (Page?.TranslationOf != null && !string.IsNullOrEmpty(Page.TranslationOf.Language))
				{
					return CultureInfo.GetCultureInfo(Page.TranslationOf.Language).Name;
				}

				return null;
			}
		}

		public string PageLanguage
		{
			get
			{
				if (Page != null && !string.IsNullOrEmpty(Page.Language))
				{
					return CultureInfo.GetCultureInfo(Page.Language).Name;
				}

				return null;
			}
		}

		public ContentItem Page { get; }

		public ContentItem ParentLink
		{
			get
			{
				if (Page != null)
				{
					return Page.Parent;
				}

				return null;
			}
		}

		public string SelectedLanguage
		{
			get
			{
				return _selectedLanguage;
			}
			set
			{
				_selectedLanguage = value;
				_selectedLanguageBranch = null;
			}
		}

		public Language SelectedLanguageBranch
		{
			get
			{
				if (_selectedLanguageBranch == null && _selectedLanguage != null)
				{
					_selectedLanguageBranch = Context.Current.LanguageManager.GetLanguage(_selectedLanguage);
					if (_selectedLanguageBranch == null)
					{
						throw new ZeusException("Invalid language '" + _selectedLanguage + "' selected.");
					}
				}
				return _selectedLanguageBranch;
			}
		}
	}
}