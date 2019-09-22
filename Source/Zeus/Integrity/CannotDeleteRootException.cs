namespace Zeus.Integrity
{
	/// <summary>
	/// Exception thrown when an attempt to remove the root item is made.
	/// </summary>
	public class CannotDeleteRootException : ZeusException
	{
		public CannotDeleteRootException()
			: base("Cannot delete root item or start page")
		{
		}

		public CannotDeleteRootException(string message) : base(message)
		{
		}

		public CannotDeleteRootException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public CannotDeleteRootException(string message, System.Exception innerException) : base(message, innerException)
		{
		}
	}
}