using System.Text.Json.Serialization;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation
{
    public class LanguageDetectionResponse
    {
        [JsonPropertyName("language")]
        public String Language { get; set; }

        [JsonPropertyName("confidence")]
        public Double Confidence { get; set; }
    }
}
