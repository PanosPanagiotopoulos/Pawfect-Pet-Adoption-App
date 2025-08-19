
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
            String healthStatus = input.HealthStatus;
            String description = input.Description;
            String gender = input.Gender;
            String breedName = input.BreedName;
            String breedDescription = input.BreedDescription;
            String animalTypeName = input.AnimalTypeName;
            String animalTypeDescription = input.AnimalTypeDescription;

            // Translate all - properly declare as Task<String>
            Task<String> healthStatusTranslated = _translationService.TranslateAsync(healthStatus, null, SupportedLanguages.English);
            Task<String> descriptionTranslated = _translationService.TranslateAsync(description, null, SupportedLanguages.English);
            Task<String> genderTranslated = _translationService.TranslateAsync(gender, null, SupportedLanguages.English);
            Task<String> breedNameTranslated = _translationService.TranslateAsync(breedName, null, SupportedLanguages.English);
            Task<String> breedDescriptionTranslated = _translationService.TranslateAsync(breedDescription, null, SupportedLanguages.English);
            Task<String> animalTypeNameTranslated = _translationService.TranslateAsync(animalTypeName, null, SupportedLanguages.English);
            Task<String> animalTypeDescriptionTranslated = _translationService.TranslateAsync(animalTypeDescription, null, SupportedLanguages.English);

            await Task.WhenAll(
                healthStatusTranslated, descriptionTranslated, genderTranslated,
                breedNameTranslated, breedDescriptionTranslated, animalTypeNameTranslated,
                animalTypeDescriptionTranslated
            );

            // Assign the results back to the input object
            input.HealthStatus = await healthStatusTranslated;
            input.Description = await descriptionTranslated;
            input.Gender = await genderTranslated;
            input.BreedName = await breedNameTranslated;
            input.BreedDescription = await breedDescriptionTranslated;
            input.AnimalTypeName = await animalTypeNameTranslated;
            input.AnimalTypeDescription = await animalTypeDescriptionTranslated;

            return input.ToSearchText();
        }
    }
}
