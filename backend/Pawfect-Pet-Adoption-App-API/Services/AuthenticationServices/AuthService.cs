using Newtonsoft.Json;

using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
	public class AuthService : IAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<AuthService> _logger;

		public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<(String, String)> RetrieveGoogleCredentials(String? authorisationCode)
		{
			GoogleTokenResponse? tokenResponse = await ExchangeCodeForAccessToken(authorisationCode);
			if (tokenResponse == null)
			{
				// LOGS //
				_logger.LogError("Αποτυχία ανταλλαγής Google Code για Access Token");
				throw new InvalidOperationException("Αποτυχία ανταλλαγής Google Code για Access Token");
			}

			GoogleUserInfo? userInfo = GetGoogleUserInfo(tokenResponse?.AccessToken).Result;
			if (userInfo == null)
			{
				// LOGS //
				_logger.LogError("Αποτυχία ανάκτησης δεδομένων χρήστη απο τη Google");
				throw new InvalidOperationException("Αποτυχία ανάκτησης δεδομένων χρήστη απο τη Google");
			}

			return (userInfo.Email, userInfo.Sub);
		}

		public async Task<GoogleTokenResponse?> ExchangeCodeForAccessToken(String? authorizationCode)
		{
			String? clientId = _configuration["Google:ClientId"];
			String? clientSecret = _configuration["Google:ClientSecret"];

			if (String.IsNullOrEmpty(clientId) || String.IsNullOrEmpty(clientSecret) || String.IsNullOrEmpty(authorizationCode))
			{
				throw new InvalidOperationException("Έλειψη clientId ή client secret ή authorisation code για exchange code -> token σε Google Provider.");
			}


			HttpClient client = new HttpClient();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

			Dictionary<String, String> requestData = new Dictionary<String, String>
			{
				{ "code", authorizationCode },
				{ "client_id", clientId },
				{ "client_secret", clientSecret },
				{ "grant_type", "authorization_code" }
			};

			request.Content = new FormUrlEncodedContent(requestData);

			HttpResponseMessage response = await client.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				throw new InvalidOperationException("Αποτυχία στο exchange google code για access token.");
			}

			String responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
		}
		public async Task<GoogleUserInfo?> GetGoogleUserInfo(String? accessToken)
		{
			if (String.IsNullOrEmpty(accessToken))
			{
				throw new InvalidOperationException("Έλειψη access token για ανάκτηση δεδομένων χρήστη απο τη Google.");
			}

			HttpClient client = new HttpClient();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v3/userinfo");
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

			HttpResponseMessage response = await client.SendAsync(request);
			if (!response.IsSuccessStatusCode)
			{
				throw new Exception("Αποτυχία απόκτησης δεδομένων χρήστη απο τη Google.");
			}

			String? responseContent = await response.Content.ReadAsStringAsync();
			return JsonConvert.DeserializeObject<GoogleUserInfo>(responseContent);
		}
	}
}
