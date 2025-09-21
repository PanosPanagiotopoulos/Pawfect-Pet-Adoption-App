using Microsoft.Extensions.AI;
using Pawfect_API.Data.Entities.Types.Embedding;

namespace Pawfect_API.Services.EmbeddingServices
{
    public interface IEmbeddingService
    {
        // Single embedding methods
        Task<Embedding<Decimal>> GenerateEmbeddingAsync(String value);
        Task<Embedding<Double>> GenerateEmbeddingAsyncDouble(String value);

        // Batch embedding methods
        Task<GeneratedEmbeddings<Embedding<Decimal>>> GenerateEmbeddingsAsync(List<String> values);
        Task<GeneratedEmbeddings<Embedding<Double>>> GenerateEmbeddingsAsyncDouble(List<String> values);

        // Aggregated embedding methods (chunking internally, returning single optimal vector)
        Task<Embedding<Decimal>> GenerateAggregatedEmbeddingAsync<TInput>(ChunkedEmbeddingInput<TInput> input)
            where TInput : class;
        Task<Embedding<Double>> GenerateAggregatedEmbeddingAsyncDouble<TInput>(ChunkedEmbeddingInput<TInput> input)
            where TInput : class;

        // Batch aggregated embedding methods
        Task<List<Embedding<Decimal>>> GenerateAggregatedEmbeddingsAsync<TInput>(ChunkedEmbeddingInput<TInput>[] inputs)
            where TInput : class;
        Task<Embedding<Double>[]> GenerateAggregatedEmbeddingsAsyncDouble<TInput>(ChunkedEmbeddingInput<TInput>[] inputs)
            where TInput : class;
    }
}
