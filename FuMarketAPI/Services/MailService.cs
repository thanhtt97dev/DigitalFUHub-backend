using System.Net;
using System.Net.Mail;
using System.Text;

namespace FuMarketAPI.Services
{
	public class MailService
	{
		private readonly IConfiguration configuration;

		public MailService(IConfiguration _configuration)
		{
			configuration = _configuration;
		}

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			string? mail = configuration["SendMail:Email"];
			string? password = configuration["SendMail:Password"];
			if(string.IsNullOrEmpty(mail) || string.IsNullOrEmpty(password)) return;

			SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
			{
				EnableSsl = true,
				Credentials = new NetworkCredential(mail, password),
			};

			MailMessage mailMessage = new MailMessage(from: mail, to: email);
			mailMessage.Subject = subject;
			mailMessage.Body = message;
			mailMessage.IsBodyHtml = true;
			await client.SendMailAsync(mailMessage);
		}
	}
}
