namespace Zeus.Admin.Plugins.ImportExport
{
	public class ContentTypeNotFoundException : ZeusException
	{
		public ContentTypeNotFoundException(string message)
			: base(message)
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
	}
}