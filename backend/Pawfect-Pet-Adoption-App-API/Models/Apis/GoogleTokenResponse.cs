using Newtonsoft.Json;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    // Payload JSON μοντέλο για την απάντηση του Google για τον χρήστη
    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public String AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public String TokenType { get; set; }
    }
}
