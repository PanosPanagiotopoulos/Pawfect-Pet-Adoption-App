using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;

using System.Text;

namespace Pawfect_Pet_Adoption_App_API.Services.SmsServices
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
			String? smsServiceUrl = _configuration.Url;
			String? apiKey = _configuration.ApiKey;

			if (String.IsNullOrWhiteSpace(smsServiceUrl) || String.IsNullOrEmpty(apiKey))
			{
				throw new InvalidDataException("Wrong configuration data found");
			}


			// Κατασκευάζουμε τον αριθμό τηλεφώνου μόνο με με τα νούμερα του και στην αρχή τον κωδικό της χώρας για την υπηρεσία
			String cleanedPhonenumber = ISmsService.ParsePhoneNumber(phoneNumber);

			String? fromPhonenumber = _configuration.From;
			if (String.IsNullOrEmpty(fromPhonenumber))
			{
				// LOGS //
				_logger.LogError("From phone number to send SMS not found.");
				throw new InvalidOperationException("From phone configuration not found at SMS Service");
			}

			// Κατασκευή payload για το SMS API
			var payload = new
			{
				messages = new[]
				{
					new
					{
						destinations = new[]
						{
							new { to = phoneNumber}
						},
                        // Τωρινό Sender ID απο την υπηρεσία
                        from = fromPhonenumber,
						text = message
					}
				}
			};

			// Serialize το payload to JSON
			StringContent content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

			// Set up the HTTP client request
			using (HttpClient client = new HttpClient())
			{
				client.DefaultRequestHeaders.Clear();
				client.DefaultRequestHeaders.Add("Authorization", $"App {apiKey}");
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				HttpResponseMessage response = await client.PostAsync(smsServiceUrl, content);

				if (!response.IsSuccessStatusCode)
				{
					String errorContent = await response.Content.ReadAsStringAsync();
					throw new Exception($"Αποτυχία αποστολής SMS. Status Code: {response.StatusCode}, Response: {errorContent}");
				}
			}
		}
	}
}
