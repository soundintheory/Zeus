using System.Xml;
using Zeus.ContentProperties;

namespace Zeus.Serialization
{
	public class PropertyCollectionXmlWriter : PropertyXmlWriter
	{
		public override void Write(ContentItem item, XmlTextWriter writer)
		{
			using (new ElementWriter("propertyCollections", writer))
			{
				foreach (var collection in item.DetailCollections.Values)
					WriteDetailCollection(writer, collection);
			}
		}

		protected virtual void WriteDetailCollection(XmlTextWriter writer, PropertyCollection collection)
		{
			using (var collectionElement = new ElementWriter("collection", writer))
			{
				collectionElement.WriteAttribute("name", collection.Name);
				foreach (var detail in collection.Details)
					WriteDetail(detail, writer);
			}
		}
	}
}