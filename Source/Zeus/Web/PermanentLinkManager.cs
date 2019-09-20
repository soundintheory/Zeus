using System;
using System.Text.RegularExpressions;

namespace Zeus.Web
{
	public class PermanentLinkManager : IPermanentLinkManager
	{
		public string ResolvePermanentLinks(string value)
		{
			const string pattern = @"href=""/?~/link/([\d]+?)""";
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);
			return regex.Replace(value, OnPatternMatched);
		}

		private string OnPatternMatched(Match match)
		{
			// Get ContentID from link.
			var contentID = Convert.ToInt32(match.Groups[1].Value);

			// Load content item and get URL.
			var contentItem = Context.Persister.Get(contentID);
			return string.Format(@"href=""{0}""", (contentItem != null) ? contentItem.Url : "#");
		}
	}
}