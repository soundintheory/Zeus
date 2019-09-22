namespace Zeus.Templates.Services.AntiSpam
{
	public class CaptchaException : ZeusException
	{
		public CaptchaException(string message) : base(message)
		{
		}

		public CaptchaException(string message, string captchaError)
			: base(message)
		{
			CaptchaError = captchaError;
		}

		public CaptchaException(string messageFormat, params object[] args) : base(messageFormat, args)
		{
		}

		public CaptchaException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		public CaptchaException() : base()
		{
		}

		public string CaptchaError { get; set; }
	}
}