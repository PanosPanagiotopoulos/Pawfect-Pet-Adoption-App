using Microsoft.Extensions.Options;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;

namespace Pawfect_Pet_Adoption_App_API.Services.TranslationServices
{
    public class TranslationService : ITranslationService
    {
        private readonly ILogger<TranslationService> _logger;
        private readonly TranslationConfig _config;
        private readonly ConcurrentDictionary<String, String> _translationCache;

        public TranslationService(
            HttpClient httpClient,
            ILogger<TranslationService> logger,
            IOptions<TranslationConfig> options)
        {
            _logger = logger;
            _config = options.Value;
            _translationCache = new ConcurrentDictionary<String, String>();
        }

        public async Task<String> TranslateAsync(String input, String sourceLang, String targetLang)
        {
            if (String.IsNullOrWhiteSpace(input)) throw new ArgumentException("Null or empty input given to translate");

            // Normalize language codes
            String normalizedSourceLang = this.NormalizeLanguageCode(sourceLang);
            String normalizedTargetLang = this.NormalizeLanguageCode(targetLang);

            if (normalizedSourceLang.Equals(normalizedTargetLang, StringComparison.OrdinalIgnoreCase))
                return input;

            // Fast check - if already in target language, return as is
            if (this.IsAlreadyInTargetLanguage(input, normalizedTargetLang))
                return input;

            // Check cache first
            String cacheKey = $"{input.GetHashCode()}|{normalizedSourceLang}|{normalizedTargetLang}";
            if (_config.EnableCaching && _translationCache.TryGetValue(cacheKey, out String? cachedResult))
            {
                _logger.LogDebug($"Translation found in cache for key: {cacheKey}");
                return cachedResult;
            }

            TranslationRequest request = new TranslationRequest
            {
                Query = input,
                Source = normalizedSourceLang,
                Target = normalizedTargetLang,
                Format = "text"
            };

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{_config.Url}/translate", request);

                if (!response.IsSuccessStatusCode)
                {
                    String errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Translation API error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Translation failed: {response.StatusCode} - {errorContent}");
                }

                TranslationResponse? result = await response.Content.ReadFromJsonAsync<TranslationResponse>();

                if (result == null || String.IsNullOrWhiteSpace(result.TranslatedText))
                {
                    _logger.LogWarning($"Empty translation result received for: {input}");
                    return input;
                }

                // Cache the result for future use
                if (_config.EnableCaching)
                {
                    _translationCache.TryAdd(cacheKey, result.TranslatedText);

                    String[] keysToRemove = _translationCache.Keys.Take(_translationCache.Count - _config.MaxCacheSize).ToArray();
                    foreach (String key in keysToRemove)
                    {
                        _translationCache.TryRemove(key, out String? _);
                    }
                }

                return result.TranslatedText;
            }
        }

        public async Task<String> DetectLanguageAsync(String text)
        {
            if (String.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty text provided for language detection");
                return _config.DefaultLanguage;
            }

            // Fast character-based detection for Greek/English
            if (this.ContainsGreekCharacters(text))
                return SupportedLanguages.Greek;

            if (this.ContainsLatinCharacters(text))
                return SupportedLanguages.English;

            // Fallback to API detection if character-based detection is inconclusive
            LanguageDetectionRequest request = new LanguageDetectionRequest
            {
                Query = text
            };

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                HttpResponseMessage response = await httpClient.PostAsJsonAsync($"{_config.Url}/detect", request);

                if (!response.IsSuccessStatusCode)
                {
                    String errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Language detection API error: {response.StatusCode} - {errorContent}");
                    return _config.DefaultLanguage;
                }

                List<LanguageDetectionResponse>? results = await response.Content.ReadFromJsonAsync<List<LanguageDetectionResponse>>();
                String detectedLanguage = results?.FirstOrDefault()?.Language ?? _config.DefaultLanguage;

                _logger.LogInformation($"Detected language '{detectedLanguage}' for text: '{text.Substring(0, Math.Min(text.Length, 50))}...'");

                return this.NormalizeLanguageCode(detectedLanguage);
            }
        }


        private Boolean ContainsGreekCharacters(String text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            // Optimized Greek character detection using Unicode ranges
            ReadOnlySpan<Char> span = text.AsSpan();
            foreach (Char c in span)
            {
                if ((c >= 0x0370 && c <= 0x03FF) || (c >= 0x1F00 && c <= 0x1FFF))
                    return true;
            }
            return false;
        }

        private Boolean ContainsLatinCharacters(String text)
        {
            if (String.IsNullOrEmpty(text))
                return false;

            // Optimized Latin character detection
            ReadOnlySpan<Char> span = text.AsSpan();
            foreach (Char c in span)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                    return true;
            }
            return false;
        }

        private Boolean IsAlreadyInTargetLanguage(String text, String targetLanguage)
        {
            if (String.IsNullOrWhiteSpace(text))
                return true;

            return targetLanguage switch
            {
                SupportedLanguages.Greek => ContainsGreekCharacters(text) && !ContainsLatinCharacters(text),
                SupportedLanguages.English => ContainsLatinCharacters(text) && !ContainsGreekCharacters(text),
                _ => false
            };
        }

        private String NormalizeLanguageCode(String languageCode)
        {
            if (String.IsNullOrWhiteSpace(languageCode))
                return _config.DefaultLanguage;

            String normalized = languageCode.ToLowerInvariant().Trim();

            // Map common variations to supported languages
            return normalized switch
            {
                "en" or "eng" or "english" => SupportedLanguages.English,
                "el" or "gr" or "gre" or "greek" => SupportedLanguages.Greek,
                _ => normalized
            };
        }
    }
}
