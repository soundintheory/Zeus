using System.Collections.Generic;
using Zeus.ContentProperties;
using Zeus.ContentTypes;

namespace Zeus.Serialization
{
	public class DefinedPropertyXmlWriter : PropertyXmlWriter
	{
		private readonly IContentTypeManager definitions;

		public DefinedPropertyXmlWriter(IContentTypeManager definitions)
		{
			this.definitions = definitions;
		}

		protected override IEnumerable<PropertyData> GetDetails(ContentItem item)
		{
			var definition = definitions.GetContentType(item.GetType());
			foreach (var detail in item.Details.Values)
			{
				foreach (var property in definition.Properties)
				{
					if (detail.Name == property.Name)
					{
						yield return detail;
						break;
					}
				}
			}
		}
	}
}