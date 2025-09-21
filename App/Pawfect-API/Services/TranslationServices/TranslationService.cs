using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.Types.Translation;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace Pawfect_API.Services.TranslationServices
{
    public class TranslationService : ITranslationService
    {
        private readonly ILogger<TranslationService> _logger;
        private readonly TranslationConfig _config;
        private readonly ConcurrentDictionary<String, String> _translationCache;

        public TranslationService(
            ILogger<TranslationService> logger,
            IOptions<TranslationConfig> options)
        {
            _logger = logger;
            _config = options.Value;
            _translationCache = new ConcurrentDictionary<String, String>();
        }

        public async Task<String> TranslateAsync(String input, String sourceLang, String targetLang)
        {
            if (String.IsNullOrWhiteSpace(input)) return "";

            // Normalize language codes
            String normalizedSourceLang = this.NormalizeLanguageCode(sourceLang);
            String normalizedTargetLang = this.NormalizeLanguageCode(targetLang);

            if (normalizedSourceLang.Equals(SupportedLanguages.Auto)) normalizedSourceLang = await this.DetectLanguageAsync(input);

            if (normalizedSourceLang.Equals(normalizedTargetLang, StringComparison.OrdinalIgnoreCase)) return input;

            StringBuilder translatedResult = new StringBuilder();
            Int32 currentIndex = 0;

            // Process input in chunks until all text is translated
            for (; currentIndex < input.Length;)
            {
                Int32 remainingLength = input.Length - currentIndex;
                Int32 chunkSize = Math.Min(_config.MaxChunkSize, remainingLength);
                String chunk = input.Substring(currentIndex, chunkSize);

                // If not the last chunk, try to break at space character
                if (currentIndex + chunkSize < input.Length)
                {
                    Int32 lastSpaceIndex = chunk.LastIndexOf(' ');
                    if (lastSpaceIndex > 0)
                    {
                        chunk = chunk.Substring(0, lastSpaceIndex);
                        chunkSize = lastSpaceIndex;
                    }
                }

                String translatedChunk = await this.TranslateChunk(chunk, normalizedSourceLang, normalizedTargetLang);
                translatedResult.Append(translatedChunk);

                // Add space if we broke at a space and it's not the last chunk
                if (currentIndex + chunkSize < input.Length && chunk.Length < _config.MaxChunkSize)
                {
                    translatedResult.Append(' ');
                    chunkSize++;
                }

                currentIndex += chunkSize;
            }

            return translatedResult.ToString();
        }

        private async Task<String> TranslateChunk(String input, String sourceLang, String targetLang)
        {
            // Check cache first at chunk level
            String cacheKey = $"{input.GetHashCode()}|{sourceLang}|{targetLang}";
            if (_config.EnableCaching && _translationCache.TryGetValue(cacheKey, out String? cachedResult))
            {
                _logger.LogDebug($"Translation found in cache for chunk: {input.Substring(0, Math.Min(input.Length, 30))}...");
                return cachedResult;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

                String encodedText = Uri.EscapeDataString(input);
                String url = $"{_config.Url}?q={encodedText}&langpair={sourceLang}|{targetLang}";

                HttpResponseMessage response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    String errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Translation API error: {response.StatusCode} - {errorContent}");
                    throw new HttpRequestException($"Translation failed: {response.StatusCode} - {errorContent}");
                }

                String responseContent = await response.Content.ReadAsStringAsync();

                TranslationResponse? result = JsonSerializer.Deserialize<TranslationResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.ResponseData == null || String.IsNullOrWhiteSpace(result.ResponseData.TranslatedText))
                {
                    _logger.LogWarning($"Empty or null translation result received for: {input}");
                    return input;
                }

                // Check if response indicates an error
                if (result.ResponseData.TranslatedText.Contains("QUERY LENGTH LIMIT EXCEEDED") ||
                    result.ResponseData.TranslatedText.Contains("QUOTA EXCEEDED") ||
                    result.ResponseData.TranslatedText.Contains("INVALID PARAMETERS") ||
                    result.ResponseData.TranslatedText.Contains("NO QUERY SPECIFIED"))
                {
                    _logger.LogWarning($"API error in response for chunk: {input.Substring(0, Math.Min(input.Length, 50))}... Error: {result.ResponseData.TranslatedText}");
                    return input; // Return original text as fallback
                }

                String translatedText = result.ResponseData.TranslatedText;

                // Cache the result at chunk level
                if (_config.EnableCaching && !String.IsNullOrWhiteSpace(translatedText))
                {
                    _translationCache.TryAdd(cacheKey, translatedText);

                    // Clean up cache if it exceeds max size
                    if (_translationCache.Count > _config.MaxCacheSize)
                    {
                        String[] keysToRemove = _translationCache.Keys.Take(_translationCache.Count - _config.MaxCacheSize).ToArray();
                        foreach (String key in keysToRemove)
                        {
                            _translationCache.TryRemove(key, out String? _);
                        }
                    }
                }

                return translatedText;
            }
        }

        private async Task<String> DetectLanguageAsync(String text)
        {
            if (String.IsNullOrWhiteSpace(text))
                return await Task.FromResult(_config.DefaultLanguage);

            // Fast character-based detection for Greek
            if (this.ContainsGreekCharacters(text))
                return await Task.FromResult(SupportedLanguages.Greek);


            return await Task.FromResult(_config.DefaultLanguage);
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

        private String NormalizeLanguageCode(String? languageCode)
        {
            if (String.IsNullOrWhiteSpace(languageCode))
                return SupportedLanguages.Auto;

            String normalized = languageCode.ToLowerInvariant().Trim();

            // Map common variations to supported languages
            return normalized switch
            {
                "en" or "eng" or "english" => SupportedLanguages.English,
                "el" or "gr" or "gre" or "greek" => SupportedLanguages.Greek,
                "auto" or "detect" => SupportedLanguages.Auto,
                _ => normalized
            };
        }
    }
}