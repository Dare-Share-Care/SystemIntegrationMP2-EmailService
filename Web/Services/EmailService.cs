using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace Web.Services
{
    public class EmailService
    {
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _password;

        public EmailService(IConfiguration configuration)
        {
            _fromEmail = configuration["EmailSettings:FromEmail"];
            _fromName = configuration["EmailSettings:FromName"];
            _password = configuration["EmailSettings:Password"];
        }

        public void SendEmail(string recipient, string subject, string body)
        {
            var fromAddress = new MailAddress(_fromEmail, _fromName);
            var toAddress = new MailAddress(recipient);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _password)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
                   {
                       Subject = subject,
                       Body = body
                   })
            {
                smtp.Send(message);
            }
        }
    }
}