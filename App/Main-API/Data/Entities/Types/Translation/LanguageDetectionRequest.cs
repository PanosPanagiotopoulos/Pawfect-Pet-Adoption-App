using System.Text.Json.Serialization;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation
{
    public class LanguageDetectionRequest
    {
        [JsonPropertyName("q")]
        public String Query { get; set; } 
    }
}
