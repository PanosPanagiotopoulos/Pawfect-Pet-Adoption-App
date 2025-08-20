using Microsoft.Extensions.Options;


using Pawfect_API.Data.Entities.Types.Apis;
using Vonage.Request;
using Vonage;


namespace Pawfect_API.Services.SmsServices
{
	public class SmsService : ISmsService
	{
		private readonly SmsApiConfig _configuration;
		private readonly ILogger<SmsService> _logger;

		public SmsService
		(
			IOptions<SmsApiConfig> configuration,
			ILogger<SmsService> logger
		)
		{
			_configuration = configuration.Value;
			_logger = logger;
		}

		public async Task SendSmsAsync(String phoneNumber, String message)
		{
			// Κατασκευάζουμε τον αριθμό τηλεφώνου μόνο με με τα νούμερα του και στην αρχή τον κωδικό της χώρας για την υπηρεσία
			String cleanedPhonenumber = ISmsService.ParsePhoneNumber(phoneNumber);

			String? fromPhonenumber = _configuration.From;
			if (String.IsNullOrEmpty(fromPhonenumber))
				throw new ArgumentException("From phone configuration not found at SMS Service");

            Credentials credentials = Credentials.FromApiKeyAndSecret(_configuration.ApiKey, _configuration.ApiSecret);
			VonageClient client = new VonageClient(credentials);

            Vonage.Messaging.SendSmsResponse response = await client.SmsClient.SendAnSmsAsync(new Vonage.Messaging.SendSmsRequest()
			{
				To = cleanedPhonenumber,
                From = fromPhonenumber,
                Text = message
            });

            // Check response
            if (response.Messages == null || response.Messages == null || response.Messages.Count() == 0)
                throw new InvalidOperationException($"No response received from SMS service for number: {cleanedPhonenumber}");

            Vonage.Messaging.SmsResponseMessage messageResponse = response.Messages[0];
            if (messageResponse.Status != "0")
                throw new InvalidOperationException($"Failed to send SMS to {cleanedPhonenumber}. Status: {messageResponse.Status}, Error: {messageResponse.ErrorText ?? "Unknown error"}");
        }
	}
}
