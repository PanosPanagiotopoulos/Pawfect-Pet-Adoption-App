using Newtonsoft.Json;

using System.Text;

namespace Pawfect_Pet_Adoption_App_API.Services.SmsServices
{
	public class SmsService : ISmsService
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly ILogger<SmsService> _logger;

		public SmsService(HttpClient httpClient, IConfiguration configuration
						   , ILogger<SmsService> logger)
		{
			_httpClient = httpClient;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SendSmsAsync(String phoneNumber, String message)
		{
			String? smsServiceUrl = _configuration["SmsService:Url"];
			String? apiKey = _configuration["SmsService:ApiKey"];

			if (String.IsNullOrWhiteSpace(smsServiceUrl) || String.IsNullOrEmpty(apiKey))
			{
				throw new InvalidDataException("Wrong configuration data found");
			}


			// Κατασκευάζουμε τον αριθμό τηλεφώνου μόνο με με τα νούμερα του και στην αρχή τον κωδικό της χώρας για την υπηρεσία
			String cleanedPhonenumber = ISmsService.ParsePhoneNumber(phoneNumber);

			String? fromPhonenumber = _configuration["SmsService:From"];
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
			_httpClient.DefaultRequestHeaders.Clear();
			_httpClient.DefaultRequestHeaders.Add("Authorization", $"App {apiKey}");
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

			HttpResponseMessage response = await _httpClient.PostAsync(smsServiceUrl, content);

			if (!response.IsSuccessStatusCode)
			{
				String errorContent = await response.Content.ReadAsStringAsync();
				throw new Exception($"Αποτυχία αποστολής SMS. Status Code: {response.StatusCode}, Response: {errorContent}");
			}

		}
	}
}
