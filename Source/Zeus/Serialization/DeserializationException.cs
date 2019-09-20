namespace Zeus.Serialization
{
	/// <summary>
	/// An exception that may be thrown when problems occur in the deseralization 
	/// of exported content data.
	/// </summary>
	public class DeserializationException : ZeusException
	{
		public DeserializationException(string message)
			: base(message)
		{
		}

		public DeserializationException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public DeserializationException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public DeserializationException() : base()
		{
		}
	}
}