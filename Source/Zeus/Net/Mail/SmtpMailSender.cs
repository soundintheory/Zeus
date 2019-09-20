using System.Net;
using System.Net.Mail;

namespace Zeus.Net.Mail
{
	public abstract class SmtpMailSender : IMailSender
	{
		public void Send(MailMessage mail)
		{
			var client = GetSmtpClient();
			client.Send(mail);
		}

		public void Send(string from, string recipients, string subject, string body)
		{
			var client = GetSmtpClient();
			client.Send(from, recipients, subject, body);
		}

		protected abstract SmtpClient GetSmtpClient();

		protected SmtpClient CreateSmtpClient(string host, int port, string user, string password)
		{
			var client = string.IsNullOrEmpty(host) ? new SmtpClient() : new SmtpClient(host, port);
			if (!string.IsNullOrEmpty(user))
			{
				client.Credentials = new NetworkCredential(user, password);
			}

			return client;
		}
	}
}