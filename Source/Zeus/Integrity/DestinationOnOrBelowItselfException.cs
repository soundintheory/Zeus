namespace Zeus.Integrity
{
	/// <summary>
	/// Exception thrown when an attempt to move an item onto or below itself is made.
	/// </summary>
	public class DestinationOnOrBelowItselfException : ZeusException
	{
		public DestinationOnOrBelowItselfException(ContentItem source, ContentItem destination)
			: base("Cannot move item to a destination onto or below itself.")
		{
			SourceItem = source;
			DestinationItem = destination;
		}

		public DestinationOnOrBelowItselfException(string message) : base(message)
		{
		}

		public DestinationOnOrBelowItselfException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public DestinationOnOrBelowItselfException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public DestinationOnOrBelowItselfException() : base()
		{
		}

		/// <summary>Gets the source item that is causing the conflict.</summary>
		public ContentItem SourceItem
		{
			get;
		}

		/// <summary>Gets the parent item already containing an item with the same name.</summary>
		public ContentItem DestinationItem
		{
			get;
		}
	}
}