using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Mistral.SDK;
using Mistral.SDK.DTOs;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingConfig _config;
        private readonly ILogger<EmbeddingService> _logger;

        public EmbeddingService
        (
            ILogger<EmbeddingService> logger,
            IOptions<EmbeddingConfig> options
        )
        {
            this._config = options.Value;
            this._logger = logger;
        }
        public async Task<Embedding<double>> GenerateEmbeddingAsyncDouble(String value) => (await this.GenerateEmbeddingsAsyncDouble([value]))?.FirstOrDefault();

        public async Task<GeneratedEmbeddings<Embedding<double>>> GenerateEmbeddingsAsyncDouble(List<String> values)
        {
            GeneratedEmbeddings<Embedding<decimal>> results = await this.GenerateEmbeddingsAsync(values);

            return new GeneratedEmbeddings<Embedding<double>>(
                results?.Select(res => new Embedding<double>(
                    new ReadOnlyMemory<double>(res.Vector.ToArray().Select(x => Convert.ToDouble(x)).ToArray())) 
                    { 
                        CreatedAt = res.CreatedAt, 
                        ModelId = res.ModelId 
                    })
                    .ToList()
              );
        }
        public async Task<Embedding<Decimal>> GenerateEmbeddingAsync(String value) => (await this.GenerateEmbeddingsAsync([value]))?.FirstOrDefault();

        public async Task<GeneratedEmbeddings<Embedding<Decimal>>> GenerateEmbeddingsAsync(List<String> values)
        {
            ArgumentNullException.ThrowIfNull(values);
            if (values.Count == 0) throw new ArgumentException("Empty values given to embed");

            MistralClient client = new MistralClient(_config.ApiKey);
            
            EmbeddingRequest request = new EmbeddingRequest(
                ModelDefinitions.MistralEmbed,
                values,
                EmbeddingRequest.EncodingFormatEnum.Float
            );

            EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);

            List<Embedding<Decimal>> results = response.Data.Select(d => new Embedding<Decimal>(new ReadOnlyMemory<Decimal>(d.Embedding.ToArray()))
            {
                 CreatedAt = DateTime.UtcNow,
                 ModelId = response.Model
            })
            .ToList();


            client.Dispose();

            return new GeneratedEmbeddings<Embedding<Decimal>>(results);
        }
    }
}
