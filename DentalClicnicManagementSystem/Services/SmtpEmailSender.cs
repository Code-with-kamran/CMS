using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace CMS.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _cfg;
        public SmtpEmailSender(IOptions<EmailSettings> options) => _cfg = options.Value;

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(to)) return;

            using var msg = new MailMessage
            {
                From = new MailAddress(_cfg.FromAddress, _cfg.FromName),
                Subject = subject ?? string.Empty,
                Body = htmlBody ?? string.Empty,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(to));

            using var smtp = new SmtpClient(_cfg.Host, _cfg.Port)
            {
                EnableSsl = _cfg.EnableSsl,                 // STARTTLS on 587
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = _cfg.UseDefaultCredentials,
                Credentials = _cfg.UseDefaultCredentials
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_cfg.Username, _cfg.Password),
                Timeout = 15000
            };

            // Note: SmtpClient supports STARTTLS (587). Implicit SSL on port 465 is not supported.
            await smtp.SendMailAsync(msg);
        }
    }
}
