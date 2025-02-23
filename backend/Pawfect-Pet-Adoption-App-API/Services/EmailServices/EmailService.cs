using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

using SendGrid;
using SendGrid.Helpers.Mail;

using Serilog;

namespace Pawfect_Pet_Adoption_App_API.Services.EmailServices
{
	public class EmailService : IEmailService
	{
		private readonly EmailApiConfig _configuration;

		public EmailService(IOptions<EmailApiConfig> configuration)
		{
			_configuration = configuration.Value;
		}

		public async Task SendEmailAsync(String email, String subject, String message)
		{
			String? apiKey = _configuration.ApiKey;
			String? fromName = _configuration.FromName;
			String? fromEmail = _configuration.FromEmail;

			if (String.IsNullOrEmpty(apiKey) || String.IsNullOrEmpty(fromName) || String.IsNullOrEmpty(fromEmail))
			{
				throw new InvalidDataException("Οι παραμέτροι για το configuration δεν βρέθηκαν");
			}

			Log.Information("Email Message = " + message);

			SendGridClient client = new SendGridClient(apiKey);
			EmailAddress from = new EmailAddress(fromEmail, fromName);
			EmailAddress to = new EmailAddress(email);
			SendGridMessage msg = MailHelper.CreateSingleEmail(from, to, fromName + " : " + subject, message, message);


			Response response = await client.SendEmailAsync(msg);

			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException($"Αποτυχία αποστολής verification email\nBody : {JsonConvert.SerializeObject(await response.Body.ReadAsStringAsync(), formatting: Formatting.Indented)}");
			}
		}
	}
}
