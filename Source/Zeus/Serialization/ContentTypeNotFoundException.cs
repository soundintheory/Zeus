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

		public ContentTypeNotFoundException(string message) : base(message)
		{
		}

		public ContentTypeNotFoundException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public ContentTypeNotFoundException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public ContentTypeNotFoundException() : base()
		{
		}

		public Dictionary<string, string> Attributes { get; }
	}
}