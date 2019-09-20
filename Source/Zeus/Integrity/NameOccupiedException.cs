using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zeus.Integrity
{
	/// <summary>
	/// An exception that is thrown when an item is saved and another sibling 
	/// (item with same parent) page already has the same name.
	/// </summary>
	public class NameOccupiedException : ZeusException
	{
		/// <summary>Gets the source item that is causing the conflict.</summary>
		public ContentItem SourceItem { get; }

		/// <summary>Gets the parent item already containing an item with the same name.</summary>
		public ContentItem DestinationItem { get; }

		/// <summary>
		/// Initializes a new instance of the NameOccupiedException.
		/// </summary>
		/// <param name="source">The source item that is causing the conflict.</param>
		/// <param name="destination">The parent item already containing an item with the same name.</param>
		public NameOccupiedException(ContentItem source, ContentItem destination)
			: base(FormatErrorMessage(source, destination))
		{
			this.SourceItem = source;
			this.DestinationItem = destination;
		}

		public NameOccupiedException(string message) : base(message)
		{
		}

		public NameOccupiedException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public NameOccupiedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public NameOccupiedException() : base()
		{
		}

		private static string FormatErrorMessage(ContentItem source, ContentItem destination)
		{
			return string.Format("An item named '{0}' already exists below '{1}'", source.Name, destination.Name);
		}
	}
}
