using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using P2P.Models;

namespace P2P.Service
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtp = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.User, "SecureEscrow"),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }

}
