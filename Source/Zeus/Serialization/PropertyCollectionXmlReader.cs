using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.ContentProperties;

namespace Zeus.Serialization
{
	public class PropertyCollectionXmlReader : XmlReader, IXmlReader
	{
		public void Read(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			foreach (var detailCollectionElement in EnumerateChildren(navigator))
			{
				ReadDetailCollection(detailCollectionElement, item, journal);
			}
		}

		protected void ReadDetailCollection(XPathNavigator navigator, ContentItem item, ReadingJournal journal)
		{
			var attributes = GetAttributes(navigator);
			var name = attributes["name"];

			foreach (var detailElement in EnumerateChildren(navigator))
			{
				ReadDetail(detailElement, item.GetDetailCollection(name, true), journal);
			}
		}

		protected virtual void ReadDetail(XPathNavigator navigator, PropertyCollection collection, ReadingJournal journal)
		{
			var attributes = GetAttributes(navigator);
			var type = attributes["typeName"].ToType();

			if (type != typeof(ContentItem))
			{
				collection.Add(Parse(navigator.Value, type));
			}
			else
			{
				var referencedItemID = int.Parse(navigator.Value);
				var referencedItem = journal.Find(referencedItemID);
				if (referencedItem != null)
				{
					collection.Add(referencedItem);
				}
				else
				{
					journal.ItemAdded += (sender, e) =>
        	{
        		if (e.AffectedItem.ID == referencedItemID)
        		{
        			collection.Add(e.AffectedItem);
        		}
        	};
				}
			}
		}
	}
}