using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Apis;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authentication;

namespace Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices
{
	public class AuthenticationService : IAuthenticationService
	{
		private readonly GoogleOauth2Config _googleOauth2Config;
		private readonly ILogger<AuthenticationService> _logger;

		public AuthenticationService
		(
			IOptions<GoogleOauth2Config> configuration,
			ILogger<AuthenticationService> logger
		)
		{
			_googleOauth2Config = configuration.Value;
			_logger = logger;
		}

		public async Task<GoogleTokenResponse?> ExchangeCodeForAccessToken(String? authorizationCode)
		{
			using (HttpClient client = new HttpClient())
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

				Dictionary<String, String> requestData = new Dictionary<String, String>
				{
					{ "code", authorizationCode },
					{ "client_id", _googleOauth2Config.ClientId },
					{ "client_secret", _googleOauth2Config.ClientSecret },
					{ "redirect_uri", _googleOauth2Config.RedirectUri },
					{ "grant_type", "authorization_code" }
				};

				request.Content = new FormUrlEncodedContent(requestData);

				HttpResponseMessage response = await client.SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					String errorContent = await response.Content.ReadAsStringAsync();
					_logger.LogError(errorContent);
					throw new InvalidOperationException("Αποτυχία στο exchange google code για access token.");
				}

				String responseContent = await response.Content.ReadAsStringAsync();
				_logger.LogInformation("Granted Google Authorisations: \n" + responseContent);
				return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
			}
		}
	}
}
