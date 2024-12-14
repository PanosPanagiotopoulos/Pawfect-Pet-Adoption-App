using Newtonsoft.Json;
using Pawfect_Pet_Adoption_App_API.Models;

namespace Pawfect_Pet_Adoption_App_API.Services
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

        public async Task<(string, string)> RetrieveGoogleCredentials(string? authorisationCode)
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

        public async Task<GoogleTokenResponse?> ExchangeCodeForAccessToken(string? authorizationCode)
        {
            string? clientId = _configuration["Google:ClientId"];
            string? clientSecret = _configuration["Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(authorizationCode))
            {
                throw new InvalidOperationException("Έλειψη clientId ή client secret ή authorisation code για exchange code -> token σε Google Provider.");
            }


            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token");

            Dictionary<string, string> requestData = new Dictionary<string, string>
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

            string responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);
        }
        public async Task<GoogleUserInfo?> GetGoogleUserInfo(string? accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
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

            string? responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GoogleUserInfo>(responseContent);
        }
    }
}
