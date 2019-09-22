namespace Zeus.Admin.Plugins.ImportExport
{
	public class InvalidXmlException : ZeusException
	{
		public InvalidXmlException(string message)
			: base(message)
		{
		}

		public InvalidXmlException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public InvalidXmlException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public InvalidXmlException() : base()
		{
		}
	}
}