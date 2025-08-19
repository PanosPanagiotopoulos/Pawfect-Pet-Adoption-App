using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using Mistral.SDK;
using Mistral.SDK.DTOs;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Embedding;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo;
using System.Numerics;
using System.Text.Json;

namespace Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly EmbeddingConfig _config;
        private readonly ILogger<EmbeddingService> _logger;
        private readonly IndexSettings _indexConfig;

        public EmbeddingService
        (
            ILogger<EmbeddingService> logger,
            IOptions<EmbeddingConfig> options,
            IOptions<MongoDbConfig> indexConfigOptions
        )
        {
            this._config = options.Value;
            this._logger = logger;
            this._indexConfig = indexConfigOptions.Value.IndexSettings;
        }
        public async Task<Embedding<Double>> GenerateEmbeddingAsyncDouble(String value) => (await this.GenerateEmbeddingsAsyncDouble([value]))?.FirstOrDefault();

        public async Task<GeneratedEmbeddings<Embedding<Double>>> GenerateEmbeddingsAsyncDouble(List<String> values)
        {
            GeneratedEmbeddings<Embedding<Decimal>> results = await this.GenerateEmbeddingsAsync(values);

            return new GeneratedEmbeddings<Embedding<Double>>(
                results?.Select(res => new Embedding<Double>(
                    new ReadOnlyMemory<Double>(res.Vector.ToArray().Select(x => Convert.ToDouble(x)).ToArray())) 
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

            List<Embedding<Decimal>> results = response.Data.Select(data => 
            new Embedding<Decimal>(new ReadOnlyMemory<Decimal>(this.NormalizeEmbedding(data.Embedding.ToArray())))
            {
                 CreatedAt = DateTime.UtcNow,
                 ModelId = response.Model
            })
            .ToList();


            client.Dispose();

            return new GeneratedEmbeddings<Embedding<Decimal>>(results);
        }

        #region Aggregated Embedding Methods
        public async Task<Embedding<Decimal>> GenerateAggregatedEmbeddingAsync<TInput>(ChunkedEmbeddingInput<TInput> input)
        where TInput : class
        {
            ArgumentNullException.ThrowIfNull(input);

            List<Embedding<Decimal>> results = await this.GenerateAggregatedEmbeddingsAsync(new[] { input });
            return results.FirstOrDefault();
        }

        public async Task<List<Embedding<Decimal>>> GenerateAggregatedEmbeddingsAsync<TInput>(ChunkedEmbeddingInput<TInput>[] inputs)
        where TInput : class
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Length == 0) throw new ArgumentException("No inputs given");

            List<Embedding<Decimal>> results = new List<Embedding<Decimal>>();

            foreach (ChunkedEmbeddingInput<TInput> input in inputs)
            {
                // Convert input to String and chunk it
                List<TextChunk> textChunks = this.CreateTextChunks(input.Content);

                if (!textChunks.Any())
                {
                    _logger.LogWarning($"No text chunks generated for input {input.SourceId}");
                    continue;
                }

                // Generate embeddings for all chunks
                List<String> chunkContents = textChunks.Select(c => c.Content).ToList();
                GeneratedEmbeddings<Embedding<Decimal>> chunkEmbeddings = await this.GenerateEmbeddingsAsync(chunkContents);

                if (chunkEmbeddings.Count != textChunks.Count)
                {
                    _logger.LogError($"Failed to embed all text chunks for input {input.SourceId}. Expected {textChunks.Count}, got {chunkEmbeddings.Count}");
                    continue;
                }

                // Create weighted chunk information for optimal aggregation
                List<WeightedChunk<Decimal>> weightedChunks = new List<WeightedChunk<Decimal>>();
                for (int i = 0; i < textChunks.Count; i++)
                {
                    TextChunk textChunk = textChunks[i];
                    Decimal[] embeddingVector = chunkEmbeddings[i].Vector.ToArray();

                    // Calculate chunk weight based on content length and position
                    Double contentWeight = this.CalculateChunkWeight(textChunk, textChunks);

                    weightedChunks.Add(new WeightedChunk<Decimal>
                    {
                        Embedding = embeddingVector,
                        Weight = contentWeight,
                        TextLength = textChunk.Content.Length,
                        Position = i,
                        StartIndex = textChunk.StartPosition,
                        EndIndex = textChunk.EndPosition
                    });
                }

                // Aggregate chunks into single optimal embedding
                Decimal[] aggregatedVector = this.AggregateChunksOptimally(weightedChunks);

                // Create final embedding result
                Embedding<Decimal> aggregatedEmbedding = new Embedding<Decimal>(new ReadOnlyMemory<Decimal>(aggregatedVector))
                {
                    CreatedAt = DateTime.UtcNow,
                    ModelId = chunkEmbeddings.FirstOrDefault()?.ModelId
                };

                results.Add(aggregatedEmbedding);
            }

            return results;
        }

        public async Task<Embedding<Double>> GenerateAggregatedEmbeddingAsyncDouble<TInput>(ChunkedEmbeddingInput<TInput> input)
        where TInput : class
        {
            ArgumentNullException.ThrowIfNull(input);

            Embedding<Double>[] results = await this.GenerateAggregatedEmbeddingsAsyncDouble(new[] { input });
            return results.FirstOrDefault();
        }

        public async Task<Embedding<Double>[]> GenerateAggregatedEmbeddingsAsyncDouble<TInput>(ChunkedEmbeddingInput<TInput>[] inputs)
        where TInput : class
        {
            List<Embedding<Decimal>> decimalResults = await this.GenerateAggregatedEmbeddingsAsync(inputs);

            return decimalResults.Select(embedding => new Embedding<Double>(
                new ReadOnlyMemory<Double>(embedding.Vector.ToArray().Select(x => Convert.ToDouble(x)).ToArray()))
            {
                CreatedAt = embedding.CreatedAt,
                ModelId = embedding.ModelId
            })
                .ToArray();
        }
        #endregion

        #region Aggregation Embedding Helpers
        private Double CalculateChunkWeight(TextChunk chunk, List<TextChunk> allChunks)
        {
            // Base weight from content length (longer chunks get higher weight)
            Double lengthWeight = Math.Log(Math.Max(1, chunk.Content.Length)) / Math.Log(2);

            // Position weight (middle chunks get slightly higher weight as they contain core content)
            Double positionWeight = 1.0;
            if (allChunks.Count > 2)
            {
                Int32 chunkIndex = allChunks.IndexOf(chunk);
                Double normalizedPosition = (Double)chunkIndex / (allChunks.Count - 1);

                // Bell curve: higher weight for middle chunks
                positionWeight = 0.7 + 0.6 * Math.Exp(-Math.Pow(normalizedPosition - 0.5, 2) / 0.2);
            }

            // Content quality weight (chunks with more meaningful content)
            Double qualityWeight = this.CalculateContentQuality(chunk.Content);

            return lengthWeight * positionWeight * qualityWeight;
        }

        private Double CalculateContentQuality(String content)
        {
            if (String.IsNullOrWhiteSpace(content)) return 0.1;

            // Higher quality for content with more unique words
            String[] words = content.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            HashSet<String> uniqueWords = new HashSet<String>(words, StringComparer.OrdinalIgnoreCase);

            Double uniquenessRatio = words.Length > 0 ? (Double)uniqueWords.Count / words.Length : 0;

            // Prefer content with good word variety and reasonable length
            Double lengthBonus = Math.Min(2.0, Math.Log(Math.Max(10, content.Length)) / Math.Log(50));

            return Math.Max(0.1, uniquenessRatio * lengthBonus);
        }

        private T[] AggregateChunksOptimally<T>(List<WeightedChunk<T>> weightedChunks) where T : INumber<T>
        {
            if (!weightedChunks.Any()) return new T[_indexConfig.Dims];

            // Method 1: Weighted Average with Attention-like Mechanism
            return this.WeightedAverageWithAttention(weightedChunks);
        }

        private T[] WeightedAverageWithAttention<T>(List<WeightedChunk<T>> weightedChunks) where T : INumber<T>
        {
            Double totalWeight = weightedChunks.Sum(c => c.Weight);
            if (totalWeight == 0) return new T[_indexConfig.Dims];

            // Normalize weights
            foreach (WeightedChunk<T> chunk in weightedChunks)
            {
                chunk.Weight /= totalWeight;
            }

            // Initialize aggregated vector
            T[] aggregatedVector = new T[_indexConfig.Dims];

            // Weighted combination of all chunk embeddings
            foreach (WeightedChunk<T> chunk in weightedChunks)
            {
                Int32 chunkDims = Math.Min(chunk.Embedding.Length, _indexConfig.Dims);

                for (Int32 i = 0; i < chunkDims; i++)
                {
                    Double chunkValue = Double.CreateChecked(chunk.Embedding[i]);
                    Double weightedValue = chunkValue * chunk.Weight;

                    Double currentValue = Double.CreateChecked(aggregatedVector[i]);
                    aggregatedVector[i] = T.CreateChecked(currentValue + weightedValue);
                }
            }

            // Apply attention mechanism: boost dimensions that are consistently important across chunks
            aggregatedVector = this.ApplyAttentionBoost(aggregatedVector, weightedChunks);

            // Final normalization to target dimensions
            return this.NormalizeEmbedding(aggregatedVector);
        }

        private T[] ApplyAttentionBoost<T>(T[] baseVector, List<WeightedChunk<T>> chunks) where T : INumber<T>
        {
            if (chunks.Count <= 1) return baseVector;

            // Calculate attention scores for each dimension
            Double[] attentionScores = new Double[_indexConfig.Dims];

            for (Int32 dim = 0; dim < _indexConfig.Dims; dim++)
            {
                Double variance = 0;
                Double mean = 0;
                Int32 validChunks = 0;

                // Calculate mean for this dimension across chunks
                foreach (WeightedChunk<T> chunk in chunks)
                {
                    if (dim < chunk.Embedding.Length)
                    {
                        mean += Double.CreateChecked(chunk.Embedding[dim]) * chunk.Weight;
                        validChunks++;
                    }
                }

                if (validChunks == 0) continue;

                // Calculate weighted variance
                foreach (WeightedChunk<T> chunk in chunks)
                {
                    if (dim < chunk.Embedding.Length)
                    {
                        Double value = Double.CreateChecked(chunk.Embedding[dim]);
                        variance += Math.Pow(value - mean, 2) * chunk.Weight;
                    }
                }

                // High variance = more discriminative = higher attention
                // Low variance = consistent across chunks = also important
                Double consistencyScore = 1.0 / (1.0 + variance); // High when low variance
                Double diversityScore = Math.Min(2.0, variance); // High when high variance

                attentionScores[dim] = 0.7 * consistencyScore + 0.3 * diversityScore;
            }

            // Apply attention scores
            T[] attentionBoostedVector = new T[_indexConfig.Dims];
            for (Int32 i = 0; i < _indexConfig.Dims; i++)
            {
                Double originalValue = Double.CreateChecked(baseVector[i]);
                Double boostedValue = originalValue * (0.8 + 0.4 * attentionScores[i]); // Boost between 0.8x and 1.2x
                attentionBoostedVector[i] = T.CreateChecked(boostedValue);
            }

            return attentionBoostedVector;
        }
        #endregion

        #region Chunk Helper Methods

        private List<TextChunk> CreateTextChunks<TInput>(TInput input) where TInput : class
        {
            // Convert input to String
            String content = input switch
            {
                String str => str,
                _ => this.ConvertToString(input)
            };

            if (String.IsNullOrWhiteSpace(content))
                return new List<TextChunk>();

            return this.ExtractChunks(content);
        }

        private List<TextChunk> ExtractChunks(String content)
        {
            // Chunk from json serialized content
            if (this.IsJsonContent(content)) return this.ChunkJsonContent(content);

            // Fallback to text chunking
            return this.ChunkTextContent(content);
        }

        private List<TextChunk> ChunkJsonContent(String jsonContent)
        {
            JsonDocument jsonDoc = JsonDocument.Parse(jsonContent);
            JsonElement root = jsonDoc.RootElement;

            // Convert each JSON property to formatted text section
            List<String> textSections = new List<String>();
            foreach (JsonProperty property in root.EnumerateObject())
            {
                String value = property.Value.GetRawText();
                if (!String.IsNullOrWhiteSpace(value)) textSections.Add(value);
            }

            // Join all sections and use text chunking
            String formattedContent = String.Join("\n", textSections);
            return this.ChunkTextContent(formattedContent);
        }

        private List<TextChunk> ChunkTextContent(String content)
        {
            List<TextChunk> chunks = new List<TextChunk>();

            if (String.IsNullOrWhiteSpace(content)) return chunks;

            // Calculate overlap size in characters
            int overlapSize = (int)(_config.ChunkSize * _config.OverlapSize / 100f);

            List<String> lines = content
                .Split(new[] { "\n\n", "\n", ". ", "! ", "? " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !String.IsNullOrWhiteSpace(line))
                .ToList();

            #pragma warning disable SKEXP0050
            List<String> textChunks = TextChunker.SplitPlainTextParagraphs(
                lines: lines,
                maxTokensPerParagraph: _config.ChunkSize,
                overlapTokens: overlapSize,
                chunkHeader: null
            )
            .Where(chunk => !String.IsNullOrWhiteSpace(chunk))
            .ToList();

            int currentPosition = 0;
            // Convert TextChunker results to our TextChunk format
            foreach (String chunk in textChunks)
            {
                chunks.Add(new TextChunk
                {
                    Content = chunk,
                    StartPosition = currentPosition,
                    EndPosition = currentPosition + chunk.Length
                });

                currentPosition += chunk.Length;
            }

            return chunks;
        }

        private String ConvertToString<TInput>(TInput input) where TInput : class
        {
            return JsonSerializer.Serialize(input) ?? input.ToString();
        }

        private Boolean IsJsonContent(String content)
        {
            if (String.IsNullOrWhiteSpace(content))
                return false;

            content = content.Trim();
            return (content.StartsWith("{") && content.EndsWith("}")) ||
                   (content.StartsWith("[") && content.EndsWith("]"));
        }

        #endregion

        #region Vector Normalization
        private T[] NormalizeEmbedding<T>(T[] vector) where T : INumber<T>
        {
            if (vector == null)
                throw new ArgumentNullException(nameof(vector));

            if (_indexConfig.Dims <= 0)
                throw new ArgumentException("Target dimensions must be positive");

            // Calculate L2 norm of original vector
            Double sumOfSquares = 0.0;
            for (int i = 0; i < vector.Length; i++)
            {
                Double val = Double.CreateChecked(vector[i]);
                sumOfSquares += val * val;
            }

            Double magnitude = Math.Sqrt(sumOfSquares);

            if (magnitude == 0.0) return new T[_indexConfig.Dims];

            // Normalize and resize in one step
            T[] result = new T[_indexConfig.Dims];
            int copyLength = Math.Min(vector.Length, _indexConfig.Dims);

            // Normalize the overlapping portion
            for (int i = 0; i < copyLength; i++)
            {
                Double normalized = Double.CreateChecked(vector[i]) / magnitude;
                result[i] = T.CreateChecked(normalized);
            }

            return result;
        }

        #endregion
    }
}
