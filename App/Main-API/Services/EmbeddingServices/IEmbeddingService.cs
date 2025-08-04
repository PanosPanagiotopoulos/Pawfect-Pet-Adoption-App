using Microsoft.Extensions.AI;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices
{
    public interface IEmbeddingService
    {
        Task<Embedding<Decimal>> GenerateEmbeddingAsync(String value);
        Task<GeneratedEmbeddings<Embedding<Decimal>>> GenerateEmbeddingsAsync(List<String> values);
        Task<Embedding<double>> GenerateEmbeddingAsyncDouble(String value);
        Task<GeneratedEmbeddings<Embedding<double>>> GenerateEmbeddingsAsyncDouble(List<String> values);
    }
}
