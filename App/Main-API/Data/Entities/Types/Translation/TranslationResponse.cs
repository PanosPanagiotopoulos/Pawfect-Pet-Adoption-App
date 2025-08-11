using System.Text.Json.Serialization;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation
{
    public class TranslationResponse
    {
        [JsonPropertyName("translatedText")]
        public String TranslatedText { get; set; }
    }
}
