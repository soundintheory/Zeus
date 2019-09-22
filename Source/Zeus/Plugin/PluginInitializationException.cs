using System;

namespace Zeus.Plugin
{
	public class PluginInitializationException : ZeusException
	{
		public PluginInitializationException(string message, Exception[] innerExceptions)
			: base(message, innerExceptions[0])
		{
			InnerExceptions = innerExceptions;
		}

		public PluginInitializationException(string message) : base(message)
		{
		}

		public PluginInitializationException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public PluginInitializationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public PluginInitializationException() : base()
		{
		}

		public Exception[] InnerExceptions { get; set; }
	}
}