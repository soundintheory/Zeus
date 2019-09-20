using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Zeus.Security;

namespace Zeus.Serialization
{
	public class AuthorizationRuleXmlReader : XmlReader, IXmlReader
	{
		public void Read(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			foreach (var authorizationElement in EnumerateChildren(navigator))
			{
				var attributes = GetAttributes(authorizationElement);
				var operation = attributes["operation"];
				var role = attributes["role"];
				var user = attributes["user"];
				var allowed = Convert.ToBoolean(attributes["allowed"]);
				item.AuthorizationRules.Add(new AuthorizationRule(item, operation, role, user, allowed));
			}
		}
	}
}