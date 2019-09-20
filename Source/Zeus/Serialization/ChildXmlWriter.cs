using System.Xml;

namespace Zeus.Serialization
{
	public class ChildXmlWriter : IXmlWriter
	{
		public virtual void Write(ContentItem item, XmlTextWriter writer)
		{
			using (new ElementWriter("children", writer))
			{
				foreach (var child in item.Children)
				{
					WriteChild(writer, child);
				}
			}
		}

		protected virtual void WriteChild(XmlTextWriter writer, ContentItem child)
		{
			using (var childElement = new ElementWriter("child", writer))
			{
				childElement.WriteAttribute("id", child.ID);
			}
		}
	}
}