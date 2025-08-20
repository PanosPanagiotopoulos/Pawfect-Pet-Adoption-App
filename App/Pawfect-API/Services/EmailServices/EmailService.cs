using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Apis;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace Pawfect_API.Services.EmailServices
{
	public class EmailService : IEmailService
	{
		private readonly EmailApiConfig _configuration;
        private readonly String _templateDirectory;

        public EmailService
		(
			IOptions<EmailApiConfig> configuration
		)
		{
			_configuration = configuration.Value;
            _templateDirectory = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails");
        }

		public async Task SendEmailAsync(String email, String subject, String message)
		{
			String? apiKey = _configuration.ApiKey;
			String? fromName = _configuration.FromName;
			String? fromEmail = _configuration.FromEmail;

			if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(fromName) || String.IsNullOrEmpty(fromEmail))
				throw new Exception("Not correct config files for email service");

			SendGridClient client = new SendGridClient(apiKey);
			EmailAddress from = new EmailAddress(fromEmail, fromName);
			EmailAddress to = new EmailAddress(email);
			SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, fromName + " : " + subject, null, message);


			Response response = await client.SendEmailAsync(msg);

			if (!response.IsSuccessStatusCode)
				throw new InvalidOperationException($"Failed to send email\nBody : {JsonConvert.SerializeObject(await response.Body.ReadAsStringAsync(), formatting: Formatting.Indented)}");
		}

        public async Task<String> GetEmailTemplateAsync(EmailType emailType, Dictionary<String, String> parameters)
        {
            String templateFileName = emailType switch
            {
                EmailType.Verification => "Verification.html",
                EmailType.Reset_Password => "Reset_Password.html",
                _ => throw new ArgumentException($"Unsupported email type: {emailType}", nameof(emailType))
            };

            String templatePath = Path.Combine(_templateDirectory, templateFileName);

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Email template file not found: {templatePath}");
            }

            String templateContent;
            try
            {
                templateContent = await File.ReadAllTextAsync(templatePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read email template: {templatePath}", ex);
            }

            if (String.IsNullOrWhiteSpace(templateContent))
            {
                throw new InvalidOperationException($"Email template is empty: {templatePath}");
            }

            // Replace placeholders with parameter values
            foreach (KeyValuePair<String, String> param in parameters)
            {
                String placeholder = $"{{{param.Key}}}";
                templateContent = templateContent.Replace(placeholder, param.Value);
            }

            return templateContent;
        }
    }
}
