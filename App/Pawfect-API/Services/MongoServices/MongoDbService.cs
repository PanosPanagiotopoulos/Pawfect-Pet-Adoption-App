using Microsoft.Extensions.Options;

using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo;
using System;

namespace Pawfect_API.Services.MongoServices
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
            await SetupSemanticTextSearchIndexAsync(animalCollection);

            // Setup other plain text indexes
            await SetupPlainTextIndexesAsync();


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
                                }
                             }
                         }
                     }
                 }
             };

            await CreateSearchIndexAsync(collection, vectorSearchIndexDefinition);
            _logger.LogInformation($"Successfully created vector search index: {_config.IndexSettings.AnimalVectorSearchIndexName}");
        }

        private async Task SetupSemanticTextSearchIndexAsync(IMongoCollection<BsonDocument> collection)
        {
            String semanticIndexName = _config.IndexSettings.AnimalSchemanticIndexName;

            if (await this.CheckIndexExistsAsync(semanticIndexName))
            {
                _logger.LogInformation($"Semantic text search index '{semanticIndexName}' already exists, skipping creation");
                return;
            }

            BsonDocument semanticTextSearchIndexDefinition = new BsonDocument
            {
                { "name", semanticIndexName },
                { "type", "search" },
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
                                        // Primary SemanticText field with comprehensive multi-language search capabilities
                                        {
                                            nameof(Data.Entities.Animal.SemanticText), new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.standard" },
                                                { "searchAnalyzer", "lucene.standard" },
                                                { "multi", new BsonDocument
                                                    {
                                                        { "exact", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.keyword" }
                                                            }
                                                        },
                                                        { "english", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.english" }
                                                            }
                                                        },
                                                        { "greek", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.greek" }
                                                            }
                                                        },
                                                        { "fuzzy", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.standard" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        // Autocomplete for semantic text with multi-language support
                                        {
                                            "semanticTextAutocomplete", new BsonDocument
                                            {
                                                { "type", "autocomplete" },
                                                { "tokenization", "edgeGram" },
                                                { "minGrams", 2 },
                                                { "maxGrams", 15 },
                                                { "foldDiacritics", true }
                                            }
                                        },
                                        {
                                            nameof(Data.Entities.Animal.Description), new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.standard" },
                                                { "multi", new BsonDocument
                                                    {
                                                        { "english", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.english" }
                                                            }
                                                        },
                                                        { "greek", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.greek" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            nameof(Data.Entities.Animal.HealthStatus), new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.standard" },
                                                { "multi", new BsonDocument
                                                    {
                                                        { "english", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.english" }
                                                            }
                                                        },
                                                        { "greek", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.greek" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        // Numeric fields for range queries (unchanged)
                                        {
                                            nameof(Data.Entities.Animal.Age), new BsonDocument
                                            {
                                                { "type", "number" },
                                                { "representation", "double" }
                                            }
                                        },
                                        {
                                            nameof(Data.Entities.Animal.Weight), new BsonDocument
                                            {
                                                { "type", "number" },
                                                { "representation", "double" }
                                            }
                                        },
                                        {
                                            nameof(Data.Entities.Animal.Gender), new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.standard" },
                                                { "multi", new BsonDocument
                                                    {
                                                        { "keyword", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.keyword" }
                                                            }
                                                        },
                                                        { "english", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.english" }
                                                            }
                                                        },
                                                        { "greek", new BsonDocument
                                                            {
                                                                { "type", "string" },
                                                                { "analyzer", "lucene.greek" }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        {
                                            nameof(Data.Entities.Animal.AdoptionStatus), new BsonDocument
                                            {
                                                { "type", "string" },
                                                { "analyzer", "lucene.keyword" }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        // Enhanced synonyms configuration using pet_synonyms collection
                        {
                            "synonyms", new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "name", "pet_synonyms" },
                                    { "analyzer", "lucene.standard" }, // Use standard analyzer for multi-language support
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

            await this.CreateSearchIndexAsync(collection, semanticTextSearchIndexDefinition);
            _logger.LogInformation($"Successfully created comprehensive multi-language semantic text search index: {semanticIndexName}");
        }

        public async Task SetupPlainTextIndexesAsync()
        {
            _logger.LogInformation("Starting multi-language plain text indexes setup for regex queries...");

            // Setup User indexes
            IMongoCollection<BsonDocument> userCollection = this.FindCollection(typeof(Data.Entities.User));

            // Check and create User multi-language text index (ONLY ONE text index per collection)
            if (!await CheckRegularIndexExistsAsync(userCollection, _config.IndexSettings.PlainTextIndexNames.UserFullNameTextIndex))
            {
                IndexKeysDefinition<BsonDocument> userTextIndexKeys = Builders<BsonDocument>.IndexKeys.Text(nameof(Data.Entities.User.FullName));
                CreateIndexOptions userTextIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.UserFullNameTextIndex,
                    DefaultLanguage = "none", // Use "none" to support both Greek and English
                    LanguageOverride = "language",
                    TextIndexVersion = 3,
                    Weights = new BsonDocument(nameof(Data.Entities.User.FullName), 10)
                };
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(userTextIndexKeys, userTextIndexOptions));
                _logger.LogInformation($"Created User multi-language text index: {_config.IndexSettings.PlainTextIndexNames.UserFullNameTextIndex}");
            }

            // Check and create User regex index
            if (!await CheckRegularIndexExistsAsync(userCollection, _config.IndexSettings.PlainTextIndexNames.UserFullNameRegexIndex))
            {
                IndexKeysDefinition<BsonDocument> userRegexIndexKeys = Builders<BsonDocument>.IndexKeys.Ascending(nameof(Data.Entities.User.FullName));
                CreateIndexOptions userRegexIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.UserFullNameRegexIndex,
                    Collation = new Collation("en", strength: CollationStrength.Primary)
                };
                await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(userRegexIndexKeys, userRegexIndexOptions));
                _logger.LogInformation($"Created User regex index: {_config.IndexSettings.PlainTextIndexNames.UserFullNameRegexIndex}");
            }

            // Setup Shelter indexes
            IMongoCollection<BsonDocument> shelterCollection = this.FindCollection(typeof(Data.Entities.Shelter));

            // Check and create Shelter multi-language text index (ONLY ONE text index per collection)
            if (!await CheckRegularIndexExistsAsync(shelterCollection, _config.IndexSettings.PlainTextIndexNames.ShelterNameTextIndex))
            {
                IndexKeysDefinition<BsonDocument> shelterTextIndexKeys = Builders<BsonDocument>.IndexKeys.Text(nameof(Data.Entities.Shelter.ShelterName));
                CreateIndexOptions shelterTextIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.ShelterNameTextIndex,
                    DefaultLanguage = "none", // Use "none" to support both Greek and English
                    LanguageOverride = "language",
                    TextIndexVersion = 3,
                    Weights = new BsonDocument(nameof(Data.Entities.Shelter.ShelterName), 10)
                };
                await shelterCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(shelterTextIndexKeys, shelterTextIndexOptions));
                _logger.LogInformation($"Created Shelter multi-language text index: {_config.IndexSettings.PlainTextIndexNames.ShelterNameTextIndex}");
            }

            // Check and create Shelter regex index
            if (!await CheckRegularIndexExistsAsync(shelterCollection, _config.IndexSettings.PlainTextIndexNames.ShelterNameRegexIndex))
            {
                IndexKeysDefinition<BsonDocument> shelterRegexIndexKeys = Builders<BsonDocument>.IndexKeys.Ascending(nameof(Data.Entities.Shelter.ShelterName));
                CreateIndexOptions shelterRegexIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.ShelterNameRegexIndex,
                    Collation = new Collation("en", strength: CollationStrength.Primary)
                };
                await shelterCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(shelterRegexIndexKeys, shelterRegexIndexOptions));
                _logger.LogInformation($"Created Shelter regex index: {_config.IndexSettings.PlainTextIndexNames.ShelterNameRegexIndex}");
            }

            // Setup File indexes
            IMongoCollection<BsonDocument> fileCollection = this.FindCollection(typeof(Data.Entities.File));

            // Check and create File multi-language text index (ONLY ONE text index per collection)
            if (!await CheckRegularIndexExistsAsync(fileCollection, _config.IndexSettings.PlainTextIndexNames.FileNameTextIndex))
            {
                IndexKeysDefinition<BsonDocument> fileTextIndexKeys = Builders<BsonDocument>.IndexKeys.Text(nameof(Data.Entities.File.Filename));
                CreateIndexOptions fileTextIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.FileNameTextIndex,
                    DefaultLanguage = "none", // Use "none" to support both Greek and English
                    LanguageOverride = "language",
                    TextIndexVersion = 3,
                    Weights = new BsonDocument(nameof(Data.Entities.File.Filename), 10)
                };
                await fileCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(fileTextIndexKeys, fileTextIndexOptions));
                _logger.LogInformation($"Created File multi-language text index: {_config.IndexSettings.PlainTextIndexNames.FileNameTextIndex}");
            }

            // Check and create File regex index
            if (!await CheckRegularIndexExistsAsync(fileCollection, _config.IndexSettings.PlainTextIndexNames.FileNameRegexIndex))
            {
                IndexKeysDefinition<BsonDocument> fileRegexIndexKeys = Builders<BsonDocument>.IndexKeys.Ascending(nameof(Data.Entities.File.Filename));
                CreateIndexOptions fileRegexIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.FileNameRegexIndex,
                    Collation = new Collation("en", strength: CollationStrength.Primary)
                };
                await fileCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(fileRegexIndexKeys, fileRegexIndexOptions));
                _logger.LogInformation($"Created File regex index: {_config.IndexSettings.PlainTextIndexNames.FileNameRegexIndex}");
            }

            // Setup AnimalType indexes
            IMongoCollection<BsonDocument> animalTypeCollection = this.FindCollection(typeof(Data.Entities.AnimalType));

            // Check and create AnimalType multi-language text index (ONLY ONE text index per collection)
            if (!await CheckRegularIndexExistsAsync(animalTypeCollection, _config.IndexSettings.PlainTextIndexNames.AnimalTypeNameTextIndex))
            {
                IndexKeysDefinition<BsonDocument> animalTypeTextIndexKeys = Builders<BsonDocument>.IndexKeys.Text(nameof(Data.Entities.AnimalType.Name));
                CreateIndexOptions animalTypeTextIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.AnimalTypeNameTextIndex,
                    DefaultLanguage = "none", // Use "none" to support both Greek and English
                    LanguageOverride = "language",
                    TextIndexVersion = 3,
                    Weights = new BsonDocument(nameof(Data.Entities.AnimalType.Name), 10)
                };
                await animalTypeCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(animalTypeTextIndexKeys, animalTypeTextIndexOptions));
                _logger.LogInformation($"Created AnimalType multi-language text index: {_config.IndexSettings.PlainTextIndexNames.AnimalTypeNameTextIndex}");
            }

            // Check and create AnimalType regex index
            if (!await CheckRegularIndexExistsAsync(animalTypeCollection, _config.IndexSettings.PlainTextIndexNames.AnimalTypeNameRegexIndex))
            {
                IndexKeysDefinition<BsonDocument> animalTypeRegexIndexKeys = Builders<BsonDocument>.IndexKeys.Ascending(nameof(Data.Entities.AnimalType.Name));
                CreateIndexOptions animalTypeRegexIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.AnimalTypeNameRegexIndex,
                    Collation = new Collation("en", strength: CollationStrength.Primary)
                };
                await animalTypeCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(animalTypeRegexIndexKeys, animalTypeRegexIndexOptions));
                _logger.LogInformation($"Created AnimalType regex index: {_config.IndexSettings.PlainTextIndexNames.AnimalTypeNameRegexIndex}");
            }

            // Setup Breed indexes
            IMongoCollection<BsonDocument> breedCollection = this.FindCollection(typeof(Data.Entities.Breed));

            // Check and create Breed multi-language text index (ONLY ONE text index per collection)
            if (!await CheckRegularIndexExistsAsync(breedCollection, _config.IndexSettings.PlainTextIndexNames.BreedNameTextIndex))
            {
                IndexKeysDefinition<BsonDocument> breedTextIndexKeys = Builders<BsonDocument>.IndexKeys.Text(nameof(Data.Entities.Breed.Name));
                CreateIndexOptions breedTextIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.BreedNameTextIndex,
                    DefaultLanguage = "none", // Use "none" to support both Greek and English
                    LanguageOverride = "language",
                    TextIndexVersion = 3,
                    Weights = new BsonDocument(nameof(Data.Entities.Breed.Name), 10)
                };
                await breedCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(breedTextIndexKeys, breedTextIndexOptions));
                _logger.LogInformation($"Created Breed multi-language text index: {_config.IndexSettings.PlainTextIndexNames.BreedNameTextIndex}");
            }

            // Check and create Breed regex index
            if (!await CheckRegularIndexExistsAsync(breedCollection, _config.IndexSettings.PlainTextIndexNames.BreedNameRegexIndex))
            {
                IndexKeysDefinition<BsonDocument> breedRegexIndexKeys = Builders<BsonDocument>.IndexKeys.Ascending(nameof(Data.Entities.Breed.Name));
                CreateIndexOptions breedRegexIndexOptions = new CreateIndexOptions
                {
                    Name = _config.IndexSettings.PlainTextIndexNames.BreedNameRegexIndex,
                    Collation = new Collation("en", strength: CollationStrength.Primary)
                };
                await breedCollection.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(breedRegexIndexKeys, breedRegexIndexOptions));
                _logger.LogInformation($"Created Breed regex index: {_config.IndexSettings.PlainTextIndexNames.BreedNameRegexIndex}");
            }

            _logger.LogInformation("Multi-language plain text indexes setup completed successfully!");
        }

        private async Task<bool> CheckRegularIndexExistsAsync(IMongoCollection<BsonDocument> collection, string indexName)
        {
            IAsyncCursor<BsonDocument> indexes = await collection.Indexes.ListAsync();
            List<BsonDocument> indexList = await indexes.ToListAsync();

            return indexList.Any(index => index["name"].AsString == indexName);
        }

        public async Task SetupSynonymsCollectionAsync()
        {
            IMongoCollection<BsonDocument> synonymsCollection = _db.GetCollection<BsonDocument>("pet_synonyms");

            // Clear existing synonyms to ensure fresh data
            await synonymsCollection.DeleteManyAsync(new BsonDocument());

            List<BsonDocument> synonymDocuments = new List<BsonDocument>();

            // Process English synonyms from config
            foreach (IndexSynonyms category in _config.IndexSettings.SynonymsBatch)
            {
                String categoryName = category.Category;
                List<String> synonymGroups = category.Synonyms;

                foreach (String synonymGroup in synonymGroups)
                {
                    String[] synonyms = synonymGroup.Split(',').Select(s => s.Trim()).ToArray();

                    synonymDocuments.Add(new BsonDocument
                    {
                        { "synonyms", new BsonArray(synonyms) },
                        { "mappingType", "equivalent" },
                        { "category", categoryName },
                        { "language", "en" }
                    });
                }
            }

            // Process Greek synonyms from config
            if (_config.IndexSettings.GreekSynonymsBatch != null)
            {
                foreach (IndexSynonyms greekCategory in _config.IndexSettings.GreekSynonymsBatch)
                {
                    String categoryName = greekCategory.Category;
                    List<String> synonymGroups = greekCategory.Synonyms;

                    foreach (String synonymGroup in synonymGroups)
                    {
                        String[] synonyms = synonymGroup.Split(',').Select(s => s.Trim()).ToArray();

                        synonymDocuments.Add(new BsonDocument
                        {
                            { "synonyms", new BsonArray(synonyms) },
                            { "mappingType", "equivalent" },
                            { "category", categoryName },
                            { "language", "el" }
                        });
                     }
                }
            }

            if (synonymDocuments.Any())
            {
                await synonymsCollection.InsertManyAsync(synonymDocuments);
                _logger.LogInformation($"Created multi-language synonyms collection with {synonymDocuments.Count} synonym groups");

                // Verify insertion
                Int64 docCount = await synonymsCollection.CountDocumentsAsync(new BsonDocument());
                _logger.LogInformation($"Verified: pet_synonyms collection contains {docCount} documents");
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

                string indexName = indexDefinition["name"].AsString;
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

        public async Task<Boolean> CheckIndexExistsAsync(string indexName)
        {
            try
            {
                IMongoCollection<BsonDocument> collection = this.FindCollection(typeof(Data.Entities.Animal));

                // Try using aggregation with $listSearchIndexes stage instead of command
                BsonArray pipeline = new BsonArray
                {
                    new BsonDocument("$listSearchIndexes", new BsonDocument())
                };

                BsonDocument aggregateCommand = new BsonDocument
                {
                    { "aggregate", collection.CollectionNamespace.CollectionName },
                    { "pipeline", pipeline },
                    { "cursor", new BsonDocument() }
                };

                BsonDocument result = await _db.RunCommandAsync<BsonDocument>(aggregateCommand);
                BsonArray cursor = result["cursor"]["firstBatch"].AsBsonArray;

                bool exists = cursor.Any(index => index["name"].AsString == indexName);
                _logger.LogInformation($"Index '{indexName}' exists: {exists}");
                return exists;
            }
            catch (MongoCommandException ex)
            {
                _logger.LogWarning($"Could not check search indexes using aggregation: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error checking index '{indexName}': {ex.Message}");
                return false;
            }
        }
        public async Task DeleteIndexIfExistsAsync(string indexName)
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

        #endregion

        #region Helpers

        private IMongoCollection<T> FindCollection<T>() => _db.GetCollection<T>(this.ExtractCollectionName(typeof(T)));

        private IMongoCollection<BsonDocument> FindCollection(Type tEntity) => _db.GetCollection<BsonDocument>(this.ExtractCollectionName(tEntity));

        private string ExtractCollectionName(Type tEntity)
        {
            string typeName = tEntity.Name;
            return typeName.EndsWith("s", StringComparison.OrdinalIgnoreCase)
                ? typeName.ToLowerInvariant()
                : typeName.ToLowerInvariant() + "s";
        }


        /// <summary>
        /// Διαγράφει όλη τη βάση. ** TESTING **
        /// </summary>
        public async Task DropAll()
        {
            List<String> collectionNames = _db.ListCollectionNames().ToList();

            foreach (String collectionName in collectionNames)
            {
                IMongoCollection<BsonDocument> collection = _db.GetCollection<BsonDocument>(collectionName);

                // Drop all regular indexes
                List<BsonDocument> indexes = collection.Indexes.List().ToList();
                foreach (BsonDocument index in indexes)
                {
                    String indexName = index["name"].AsString;
                    if (indexName != "_id_")
                    {
                        collection.Indexes.DropOne(indexName);
                    }
                }

                // Delete all documents
                collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);
            }

            #endregion
        }
    }
}
