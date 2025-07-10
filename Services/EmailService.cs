using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using prjetax.Models;
using PrjEtax.Models;

namespace PrjEtax.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_smtp.Server, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
                EnableSsl = true
            };

            var msg = new MailMessage(from: _smtp.User, to: toEmail, subject, body)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(msg);
        }
    }
}
