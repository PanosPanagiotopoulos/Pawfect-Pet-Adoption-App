using System.Text.Json.Serialization;

namespace Pawfect_API.Data.Entities.Types.Translation
{
    public class TranslationResponse
    {
        [JsonPropertyName("responseData")]
        public TranslationResponseData? ResponseData { get; set; }

        [JsonPropertyName("responseStatus")]
        public Int32 ResponseStatus { get; set; }

        [JsonPropertyName("responseDetails")]
        public String? ResponseDetails { get; set; }
    }

    public class TranslationResponseData
    {
        [JsonPropertyName("translatedText")]
        public String TranslatedText { get; set; } 

        [JsonPropertyName("match")]
        public Double Match { get; set; }
    }
}
