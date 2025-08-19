using Pawfect_Pet_Adoption_App_API.Models.Animal;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors
{
    public interface IAnimalEmbeddingValueExtractor : IEmbeddingValueExtractor<AnimalSearchDataModel, String, String> { }
}
