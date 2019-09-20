using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Zeus.BaseLibrary.ExtensionMethods;

namespace Zeus.Serialization
{
	/// <summary>
	/// Reads a content detail from the input navigator.
	/// </summary>
	public class PropertyXmlReader : XmlReader, IXmlReader
	{
		public void Read(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			foreach (var detailElement in EnumerateChildren(navigator))
				ReadDetail(detailElement, item, journal);
		}

		protected virtual void ReadDetail(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			var attributes = GetAttributes(navigator);
			var type = attributes["typeName"].ToType();

			var name = attributes["name"];

			if (!typeof(ContentItem).IsAssignableFrom(type))
			{
				item[name] = Parse(navigator.Value, type);
			}
			else
			{
				var referencedItemID = int.Parse(navigator.Value);
				var referencedItem = journal.Find(referencedItemID);
				if (referencedItem != null)
					item[name] = referencedItem;
				else
					journal.ItemAdded += (sender, e) =>
        	{
        		if (e.AffectedItem.ID == referencedItemID)
        			item[name] = e.AffectedItem;
        	};
			}
		}
	}
}