using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace HoaIdentityServer.Services
{
    public class EmailSender : IEmailSender
    {
        private IConfiguration _configuration;
        private SendGridClient client;
        private const string SenderEmail = "hoa@hoa.com";
        private const string SenderEmailName = "Generic HOA";

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
            client = new SendGridClient(_configuration["SendGrid:ApiKey"]);
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new SendGridMessage()
            {
                From = new EmailAddress(SenderEmail, SenderEmailName),
                Subject = subject,
                HtmlContent = htmlMessage
            };

            message.AddTo(new EmailAddress(email));

            message.SetClickTracking(false, false);

            return client.SendEmailAsync(message);
        }
    }
}
