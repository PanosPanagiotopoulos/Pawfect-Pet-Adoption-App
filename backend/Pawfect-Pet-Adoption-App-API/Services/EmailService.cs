using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            string? apiKey = _configuration["SendGrid:ApiKey"];
            string? fromName = _configuration["SendGrid:FromName"];
            string? fromEmail = _configuration["SendGrid:FromEmail"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromName) || string.IsNullOrEmpty(fromEmail))
            {
                throw new InvalidDataException("Οι παραμέτροι για το configuration δεν βρέθηκαν");
            }

            SendGridClient client = new SendGridClient(apiKey);
            EmailAddress from = new EmailAddress(fromEmail, fromName);
            EmailAddress to = new EmailAddress(email);
            SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, "Pawfect : " + subject, message, message);

            Response response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Αποτυχία αποστολής verification email\nBody : {JsonConvert.SerializeObject(response.Body, formatting: Formatting.Indented)}");
            }
        }
    }
}
