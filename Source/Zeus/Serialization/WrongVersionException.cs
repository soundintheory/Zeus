namespace Zeus.Serialization
{
	public class WrongVersionException : DeserializationException
	{
		public WrongVersionException(string message)
			: base(message)
		{
		}

		public WrongVersionException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public WrongVersionException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public WrongVersionException() : base()
		{
		}
	}
}