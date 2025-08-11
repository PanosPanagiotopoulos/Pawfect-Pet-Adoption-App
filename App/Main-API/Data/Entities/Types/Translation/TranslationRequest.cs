using System.Text.Json.Serialization;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation
{
    public class TranslationRequest
    {
        [JsonPropertyName("q")]
        public String Query { get; set; } 
        [JsonPropertyName("source")]
        public String Source { get; set; } 

        [JsonPropertyName("target")]
        public String Target { get; set; } 

        [JsonPropertyName("format")]
        public String Format { get; set; } 
    }
}
