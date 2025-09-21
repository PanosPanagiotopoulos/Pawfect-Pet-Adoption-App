using Pawfect_API.Models.Animal;

namespace Pawfect_API.Services.EmbeddingServices.AnimalEmbeddingValueExtractors
{
    public interface IAnimalEmbeddingValueExtractor : IEmbeddingValueExtractor<AnimalSearchDataModel, String, String> { }
}
