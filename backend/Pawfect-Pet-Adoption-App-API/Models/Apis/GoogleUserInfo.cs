using Newtonsoft.Json;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    // Μοντέλο JSON για τα δεδομένα χρήστη του Google
    public class GoogleUserInfo
    {
        [JsonProperty("sub")]
        public String Sub { get; set; }  // Μοναδικό Google ID

        [JsonProperty("email")]
        public String Email { get; set; }
    }
}
