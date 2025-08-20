
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Services.TranslationServices;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors
{
    public class AnimalEmbeddingValueExtractor : IAnimalEmbeddingValueExtractor
    {
        private readonly ITranslationService _translationService;
        private readonly ILogger<AnimalEmbeddingValueExtractor> _logger;

        public AnimalEmbeddingValueExtractor
        (
            ITranslationService translationService,
            ILogger<AnimalEmbeddingValueExtractor> logger
        )
        {
            this._translationService = translationService;
            this._logger = logger;
        }

        public async Task<String> ExtractValue(AnimalSearchDataModel input)
        {
            String[] translations = await Task.WhenAll(
               _translationService.TranslateAsync(input.HealthStatus, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.Description, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.Gender, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.BreedName, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.BreedDescription, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.AnimalTypeName, null, SupportedLanguages.English),
               _translationService.TranslateAsync(input.AnimalTypeDescription, null, SupportedLanguages.English)
            );

            // Assign the results back to the input object
            input.HealthStatus = translations[0];
            input.Description = translations[1];
            input.Gender = translations[2];
            input.BreedName = translations[3];
            input.BreedDescription = translations[4];
            input.AnimalTypeName = translations[5];
            input.AnimalTypeDescription = translations[6];

            return input.ToSearchText();
        }
    }
}
