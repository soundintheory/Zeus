using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Ninject;
using Zeus.ContentTypes;
using Zeus.Web;

namespace Zeus.Serialization
{
	public class ItemXmlReader : XmlReader
	{
		private readonly IContentTypeManager definitions;
		private readonly IDictionary<string, IXmlReader> readers;

		[Inject]
		public ItemXmlReader(IContentTypeManager definitions)
			: this(definitions, DefaultReaders())
		{
		}

		public ItemXmlReader(IContentTypeManager definitions, IDictionary<string, IXmlReader> readers)
		{
			IgnoreMissingTypes = true;
			this.definitions = definitions;
			this.readers = readers;
		}

		public bool IgnoreMissingTypes { get; set; }

		private static IDictionary<string, IXmlReader> DefaultReaders()
		{
			IDictionary<string, IXmlReader> readers = new Dictionary<string, IXmlReader>();
			readers["properties"] = new PropertyXmlReader();
			readers["propertyCollections"] = new PropertyCollectionXmlReader();
			readers["authorizationRules"] = new AuthorizationRuleXmlReader();
			readers["languageSettings"] = new LanguageSettingXmlReader();
			return readers;
		}

		public virtual IImportRecord Read(XPathNavigator navigator)
		{
			if (navigator == null)
			{
				throw new ArgumentNullException(nameof(navigator));
			}

			var journal = new ReadingJournal();
			foreach (var itemElement in EnumerateChildren(navigator))
			{
				try
				{
					var item = ReadSingleItem(itemElement, journal);
					journal.Report(item);
				}
				catch (ContentTypeNotFoundException ex)
				{
					journal.Error(ex);
					if (!IgnoreMissingTypes)
					{
						throw;
					}
				}
			}
			return journal;
		}

		public virtual ContentItem ReadSingleItem(XPathNavigator navigator, ReadingJournal journal)
		{
			if (navigator.LocalName != "item")
			{
				throw new DeserializationException("Expected element 'item' but was '" + navigator.LocalName + "'");
			}

			var attributes = GetAttributes(navigator);
			var item = CreateInstance(attributes);
			ReadDefaultAttributes(attributes, item, journal);

			foreach (var current in EnumerateChildren(navigator))
			{
				if (readers.ContainsKey(current.LocalName))
				{
					readers[current.LocalName].Read(current, item, journal);
				}
			}

			return item;
		}

		protected virtual void ReadDefaultAttributes(Dictionary<string, string> attributes, ContentItem item, ReadingJournal journal)
		{
			item.Created = ToNullableDateTime(attributes["created"]).Value;
			item.Expires = ToNullableDateTime(attributes["expires"]);
			item.ID = Convert.ToInt32(attributes["id"]);
			item.Name = attributes["name"];
			item.Published = ToNullableDateTime(attributes["published"]);
			item.SavedBy = attributes["savedBy"];
			item.SortOrder = Convert.ToInt32(attributes["sortOrder"]);
			item.Title = attributes["title"];
			item.Updated = ToNullableDateTime(attributes["updated"]).Value;
			item.Visible = Convert.ToBoolean(attributes["visible"]);
			if (attributes.ContainsKey("zoneName"))
			{
				((WidgetContentItem) item).ZoneName = attributes["zoneName"];
			}

			HandleTranslationRelation(item, attributes["translationOf"], journal);
			if (!string.IsNullOrEmpty(attributes["language"]))
			{
				item.Language = attributes["language"];
			}

			HandleParentRelation(item, attributes["parent"], journal);
		}

		protected virtual void HandleParentRelation(ContentItem item, string parent, ReadingJournal journal)
		{
			if (!string.IsNullOrEmpty(parent))
			{
				var parentID = int.Parse(parent);
				var parentItem = journal.Find(parentID);
				item.AddTo(parentItem);
			}
		}

		protected virtual void HandleTranslationRelation(ContentItem item, string translationOf, ReadingJournal journal)
		{
			if (!string.IsNullOrEmpty(translationOf))
			{
				var translationOfID = int.Parse(translationOf);
				var masterItem = journal.Find(translationOfID);
				item.TranslationOf = masterItem;
			}
		}

		private ContentItem CreateInstance(Dictionary<string, string> attributes)
		{
			var definition = FindDefinition(attributes);
			return definitions.CreateInstance(definition.ItemType, null);
		}

		protected virtual ContentType FindDefinition(Dictionary<string, string> attributes)
		{
			var discriminator = attributes["discriminator"];
			foreach (var d in definitions.GetContentTypes())
			{
				if (d.Discriminator == discriminator)
				{
					return d;
				}
			}

			var title = attributes["title"];
			var name = attributes["name"];
			throw new ContentTypeNotFoundException(string.Format("No definition found for '{0}' with name '{1}' and discriminator '{2}'", title, name, discriminator), attributes);
		}
	}
}