using System.IO;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using prjetax.Models;

namespace PrjEtax.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        // overload nhận IFormFile
        public async Task SendEmailAsync(string to, string subject, string body, IFormFile attachment)
        {
            byte[] bytes = null;
            string name = null;

            if (attachment != null && attachment.Length > 0)
            {
                name = attachment.FileName;
                using var ms = new MemoryStream();
                await attachment.CopyToAsync(ms);
                bytes = ms.ToArray();
               

            }

            await SendEmailAsync(to, subject, body, bytes, name);
        }

        // phương thức gốc dùng byte[]
        public async Task SendEmailAsync(string to, string subject, string body, byte[] attachment = null, string attachmentName = null)
        {
            using var client = new SmtpClient(_smtp.Server, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.User, _smtp.Password),
                EnableSsl = true
            };

            using var msg = new MailMessage(_smtp.User, to, subject, body);

            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
            {
                var stream = new MemoryStream(attachment);
                msg.Attachments.Add(new Attachment(stream, attachmentName));
            }

            await client.SendMailAsync(msg);
        }
    }
}
