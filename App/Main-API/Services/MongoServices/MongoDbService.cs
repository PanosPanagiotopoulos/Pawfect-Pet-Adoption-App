using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo;

namespace Main_API.Services.MongoServices
{
	/// <summary>
	///   Εισαγωγή της βάσης στο πρόγραμμα
	///   Γενική μέθοδος για χρήση οποιουδήποτε collection
	/// </summary>
	public class MongoDbService
	{
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<MongoDbService> _logger;
        private readonly MongoDbConfig _config;

        public IMongoDatabase _db { get; }

		public MongoDbService
		(
			IOptions<MongoDbConfig> settings, 
			IMongoClient client,
            IHttpContextAccessor httpContextAccessor,
            ILogger<MongoDbService> logger,
            IOptions<MongoDbConfig> options
		)
		{
			_db = client.GetDatabase(settings.Value.DatabaseName);
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _config = options.Value;
        }
        public IMongoCollection<T> GetCollection<T>()
		{
            IMongoCollection<T> collection = this.FindCollection<T>();

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<T>(collection, _httpContextAccessor);
        }

        public IMongoCollection<BsonDocument> GetCollection(Type tEntity)
        {
            IMongoCollection<BsonDocument> collection = this.FindCollection(tEntity);

            if (collection == null) throw new InvalidOperationException("No collection found");

            return new SessionScopedMongoCollection<BsonDocument>(collection, _httpContextAccessor);
        }

        #region Indexes
        public async Task SetupSearchIndexesAsync()
        {
            _logger.LogInformation("Starting MongoDB Atlas Search Index setup...");

            IMongoCollection<BsonDocument> animalCollection = this.FindCollection(typeof(Data.Entities.Animal));

            // Setup Vector Search Index
            await SetupVectorSearchIndexAsync(animalCollection);

            // Setup search synonyms for schemantic search
            await SetupSynonymsCollectionAsync();

            // Setup Text Search Index  
            await SetupTextSearchIndexAsync(animalCollection);

            // TODO: Setup other plain text indexes


            _logger.LogInformation("MongoDB Atlas Search Index setup completed successfully!");
        }

        private async Task SetupVectorSearchIndexAsync(IMongoCollection<BsonDocument> collection)
        {
            // Check if index already exists
            if (await this.CheckIndexExistsAsync(_config.IndexSettings.AnimalVectorSearchIndexName))
            {
                _logger.LogInformation($"Vector search index '{_config.IndexSettings.AnimalVectorSearchIndexName}' already exists, skipping creation");
                return;
            }

            BsonDocument vectorSearchIndexDefinition = new BsonDocument
             {
                 {
                     "name", _config.IndexSettings.AnimalVectorSearchIndexName
                 },
                 {
                     "type", "vectorSearch"
                 },
                 {
                     "definition", new BsonDocument
                     {
                         {
                             "fields", new BsonArray
                             {
                                 new BsonDocument
                                 {
                                     { "type", "vector" },
                                     { "path", nameof(Data.Entities.Animal.Embedding) },
                                     { "numDimensions", _config.IndexSettings.Dims },
                                     { "similarity", "cosine" }
                                 },
                             }
                         }
                     }
                 }
             };

            await CreateSearchIndexAsync(collection, vectorSearchIndexDefinition);
            _logger.LogInformation($"Successfully created vector search index: {_config.IndexSettings.AnimalVectorSearchIndexName}");
        }

        private async Task SetupTextSearchIndexAsync(IMongoCollection<BsonDocument> collection)
        {
            // Check if index already exists
            if (await this.CheckIndexExistsAsync(_config.IndexSettings.AnimalSchemanticIndexName))
            {
                _logger.LogInformation($"Enhanced text search index '{_config.IndexSettings.AnimalSchemanticIndexName}' already exists, skipping creation");
                return;
            }

            BsonDocument textSearchIndexDefinition = new BsonDocument
            {
                {
                    "name", _config.IndexSettings.AnimalSchemanticIndexName
                },
                {
                    "type", "search"
                },
                {
                    "definition", new BsonDocument
                    {
                        {
                            "mappings", new BsonDocument
                            {
                                { "dynamic", false },
                                {
                                    "fields", new BsonDocument
                                    {
                                        // Name field with multi-field variants (STRING ONLY)
                                        {
                                            "name", new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.standard" },
                                                { "searchAnalyzer", "lucene.standard" },
                                                { "multi", new BsonDocument
                                                    {
                                                        // Exact match for precise name searches
                                                        { "exact", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.keyword" }
                                                            }
                                                        },
                                                        // English variant for better semantic understanding
                                                        { "english", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.english" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "nameAutocomplete", new BsonDocument
                                            {
                                                { "type", "autocomplete" },
                                                { "analyzer", "lucene.standard" },
                                                { "tokenization", "edgeGram" },
                                                { "minGrams", 2 },
                                                { "maxGrams", 15 }
                                            }
                                        },
                                        // Description - primary semantic field
                                        {
                                            "description", new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.english" }, // Best for natural language
                                                { "searchAnalyzer", "lucene.english" },
                                                { "multi", new BsonDocument
                                                    {
                                                        // Standard for exact phrases
                                                        { "standard", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.standard" }
                                                            }
                                                        },
                                                        // Keyword for exact description matches
                                                        { "exact", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.keyword" }
                                                            }
                                                        },
                                                        // Stemmed for better semantic matching
                                                        { "stemmed", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.simple" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            "descriptionAutocomplete", new BsonDocument
                                            {
                                                { "type", "autocomplete" },
                                                { "analyzer", "lucene.english" },
                                                { "tokenization", "edgeGram" },
                                                { "minGrams", 3 },
                                                { "maxGrams", 20 }
                                            }
                                        },
                                        // Health status with semantic understanding
                                        {
                                            "healthStatus", new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.english" },
                                                { "searchAnalyzer", "lucene.english" },
                                                { "multi", new BsonDocument
                                                    {
                                                        { "exact", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.keyword" }
                                                            }
                                                        },
                                                        { "standard", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.standard" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        // Numeric fields for precise range queries
                                        {
                                            "age", new BsonDocument
                                            {
                                                { "type", "number" },
                                                { "representation", "double" }
                                            }
                                        },
                                        {
                                            "weight", new BsonDocument
                                            {
                                                { "type", "number" },
                                                { "representation", "double" }
                                            }
                                        },
                                        // Gender with exact matching
                                        {
                                            "gender", new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.keyword" } // Exact match for enums
                                            }
                                        },
                                        // Additional searchable fields for better coverage
                                        {
                                            "adoptionStatus", new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.keyword" }
                                            }
                                        },
                                        // Date fields for temporal searches
                                        {
                                            "createdAt", new BsonDocument
                                            {
                                                { "type", "date" }
                                            }
                                        },
                                        {
                                            "updatedAt", new BsonDocument
                                            {
                                                { "type", "date" }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        // Enhanced synonyms configuration
                        {
                            "synonyms", new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "name", "pet_synonyms" },
                                    { "analyzer", "lucene.standard" },
                                    { "source", new BsonDocument
                                        {
                                            { "collection", "pet_synonyms" }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            await CreateSearchIndexAsync(collection, textSearchIndexDefinition);
            _logger.LogInformation($"Successfully created enhanced semantic search index: {_config.IndexSettings.AnimalSchemanticIndexName}");
        }

        public async Task SetupSynonymsCollectionAsync()
        {
            IMongoCollection<BsonDocument> synonymsCollection = _db.GetCollection<BsonDocument>("pet_synonyms");

            // Clear existing synonyms to ensure fresh data
            await synonymsCollection.DeleteManyAsync(new BsonDocument());

            List<BsonDocument> synonymDocuments = new List<BsonDocument>();

            // Process synonyms from config
            foreach (IndexSynonyms category in _config.IndexSettings.SynonymsBatch)
            {
                String categoryName = category.Category;
                List<String> synonymGroups = category.Synonyms;

                foreach (String synonymGroup in synonymGroups)
                {
                    String[] synonyms = synonymGroup.Split(',').Select(s => s.Trim()).ToArray();

                    synonymDocuments.Add(new BsonDocument
            {
                { "input", new BsonArray(synonyms) },
                { "category", categoryName },
                { "mappingType", "equivalent" }
            });
                }
            }

            if (synonymDocuments.Any())
            {
                await synonymsCollection.InsertManyAsync(synonymDocuments);
                _logger.LogInformation($"Created enhanced synonyms collection with {synonymDocuments.Count} synonym groups across {_config.IndexSettings.SynonymsBatch.Count} categories");
            }
        }

        private async Task CreateSearchIndexAsync(IMongoCollection<BsonDocument> collection, BsonDocument indexDefinition)
        {
            try
            {
                BsonDocument command = new BsonDocument
                 {
                     { "createSearchIndexes", collection.CollectionNamespace.CollectionName },
                     { "indexes", new BsonArray { indexDefinition } }
                 };

                await _db.RunCommandAsync<BsonDocument>(command);

                String indexName = indexDefinition["name"].AsString;
                _logger.LogInformation($"Search index creation initiated: {indexName}");

                _logger.LogInformation($"Note: Search index '{indexName}' is being built asynchronously and may take a few minutes to become available");
            }
            catch (MongoCommandException ex)
            {
                if (ex.Code == 68) // CommandNotFound - likely not Atlas or Atlas Search not enabled
                {
                    _logger.LogWarning("Atlas Search not available. Please ensure you're using MongoDB Atlas with Search enabled.");
                    throw new InvalidOperationException("MongoDB Atlas Search is required for this functionality. Please deploy to Atlas and enable Search.", ex);
                }
                else if (ex.CodeName == "IndexAlreadyExists")
                {
                    var indexName = indexDefinition["name"].AsString;
                    _logger.LogInformation($"Search index '{indexName}' already exists");
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<Boolean> CheckIndexExistsAsync(String indexName)
        {
            try
            {
                IMongoCollection<BsonDocument> collection = this.FindCollection(typeof(Data.Entities.Animal));

                BsonDocument command = new BsonDocument
                 {
                     { "listSearchIndexes", collection.CollectionNamespace.CollectionName }
                 };

                BsonDocument result = await _db.RunCommandAsync<BsonDocument>(command);
                BsonArray cursor = result["cursor"]["firstBatch"].AsBsonArray;

                return cursor.Any(index => index["name"].AsString == indexName);
            }
            catch (MongoCommandException)
            {
                return false;
            }
        }

        public async Task DeleteIndexIfExistsAsync(String indexName)
        {
            if (!await CheckIndexExistsAsync(indexName))
                return;

            IMongoCollection<BsonDocument> collection = this.FindCollection(typeof(Data.Entities.Animal));

            BsonDocument command = new BsonDocument
             {
                 { "dropSearchIndex", collection.CollectionNamespace.CollectionName },
                 { "name", indexName }
             };

            await _db.RunCommandAsync<BsonDocument>(command);
            _logger.LogInformation($"Deleted search index: {indexName}");
        }

        public async Task<Boolean> ValidateSearchIndexesAsync()
        {
            Boolean vectorIndexExists = await CheckIndexExistsAsync("animals_vector_search_index");
            Boolean textIndexExists = await CheckIndexExistsAsync("animals_text_search_index");

            if (vectorIndexExists && textIndexExists)
            {
                _logger.LogInformation("All search indexes are available and ready");
                return true;
            }

            if (!vectorIndexExists)
                _logger.LogWarning("Vector search index not found or not ready yet");

            if (!textIndexExists)
                _logger.LogWarning("Text search index not found or not ready yet");

            return false;
        }
        #endregion

        #region Helpers

        private IMongoCollection<T> FindCollection<T>() => _db.GetCollection<T>(this.ExtractCollectionName(typeof(T)));

        private IMongoCollection<BsonDocument> FindCollection(Type tEntity) => _db.GetCollection<BsonDocument>(this.ExtractCollectionName(tEntity));

        private String ExtractCollectionName(Type tEntity)
        {
            String typeName = tEntity.Name;
            return typeName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? typeName.ToLowerInvariant()
                : typeName.ToLowerInvariant() + "s";
        }


        /// <summary>
        /// Διαγράφει όλη τη βάση. ** TESTING **
        /// </summary>
        public void DropAllCollections()
		{
			// Get the list of collections in the database
            List<String> collectionNames = _db.ListCollectionNames().ToList();

            foreach (String collectionName in collectionNames)
			{
				IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);
				collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
			}
		}

        #endregion
    }

}
