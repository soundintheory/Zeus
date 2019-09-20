using System.Collections.Generic;

namespace Zeus.Serialization
{
	public class ContentTypeNotFoundException : DeserializationException
	{
		public ContentTypeNotFoundException(string message, Dictionary<string, string> attributes)
			: base(message)
		{
			this.Attributes = attributes;
		}

		public Dictionary<string, string> Attributes { get; }
	}
}