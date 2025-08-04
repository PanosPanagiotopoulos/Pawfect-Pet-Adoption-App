using MongoDB.Bson;
using MongoDB.Driver;

using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Exceptions;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.MongoServices;
using Pawfect_Pet_Adoption_App_API.Services.EmbeddingServices;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Main_API.Query.Queries
{
	public class AnimalQuery : BaseQuery<Data.Entities.Animal>
	{
        private readonly IEmbeddingService _embeddingService;
        private readonly MongoDbConfig _config;

        public AnimalQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver authorizationContentResolver,
            IEmbeddingService _embeddingService,
            IOptions<MongoDbConfig> options

        ) : base(mongoDbService, AuthorizationService, authorizationContentResolver, claimsExtractor)
        {
            this._embeddingService = _embeddingService;
            this._config = options.Value;
        }

        // Λίστα από IDs ζώων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα από IDs καταφυγίων για φιλτράρισμα
        public List<String>? ShelterIds { get; set; }

		// Λίστα από IDs φυλών για φιλτράρισμα
		public List<String>? BreedIds { get; set; }

		// Λίστα από IDs τύπων για φιλτράρισμα
		public List<String>? TypeIds { get; set; }

		// Λίστα από καταστάσεις υιοθεσίας για φιλτράρισμα
		public List<AdoptionStatus>? AdoptionStatuses { get; set; }
        public List<Gender>? Genders { get; set; }
        public double? AgeFrom { get; set; }
        public double? AgeTo { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

       
        public AnimalQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        #region Filters
        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Animal> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override async Task<FilterDefinition<Data.Entities.Animal>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Animal> builder = Builders<Data.Entities.Animal>.Filter;
            FilterDefinition<Data.Entities.Animal> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των ζώων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Animal.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
            if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.ShelterId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των φυλών
			if (BreedIds != null && BreedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = BreedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.BreedId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των τύπων
			if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Animal.AnimalTypeId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τις καταστάσεις υιοθεσίας
			if (AdoptionStatuses != null && AdoptionStatuses.Any())
			{
				filter &= builder.In(animal => animal.AdoptionStatus, AdoptionStatuses);
			}

            // Εφαρμόζει φίλτρο για τις καταστάσεις υιοθεσίας
            if (Genders != null && Genders.Any())
            {
                filter &= builder.In(animal => animal.Gender, Genders);
            }

            if (AgeFrom.HasValue)
            {
                filter &= builder.Gte(animal => animal.Age, AgeFrom.Value);
            }

            if (AgeTo.HasValue)
            {
                filter &= builder.Lte(animal => animal.Age, AgeTo.Value);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreateFrom.HasValue)
			{
				filter &= builder.Gte(animal => animal.CreatedAt, CreateFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(animal => animal.CreatedAt, CreatedTill.Value);
			}

            // Εφαρμόζει φίλτρο για complex search. Εδώ θα επικοινωνεί με τον Search Server για την AI-based αναζήτηση
            if (!String.IsNullOrWhiteSpace(Query))
            {
                FilterDefinition<Data.Entities.Animal> searchFilter = await this.ApplyHybridSearchFilter(builder);
                filter &= searchFilter;
            }


            return await Task.FromResult(filter);
		}

        private async Task<FilterDefinition<Data.Entities.Animal>> ApplyHybridSearchFilter(FilterDefinitionBuilder<Data.Entities.Animal> builder)
        {
            // Get vector search results (70% weight)
            List<(String id, double score)> vectorResults = await this.PerformVectorSearchAsync();

            // Get text search results (30% weight)
            List<(String id, double score)> textResults = await this.PerformTextSearchAsync();

            // Combine results and get ranked IDs
            List<String> rankedIds = this.CombineSearchResultsToIds(vectorResults, textResults);

            // If no search results found, return filter that matches nothing
            if (!rankedIds.Any()) return builder.Empty;

            // Convert ranked IDs to ObjectIds for MongoDB filter
            List<ObjectId> objectIds = rankedIds
                .Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty)
                .Where(id => id != ObjectId.Empty)
                .ToList();

            // Return filter that matches the ranked IDs
            return builder.In(nameof(Data.Entities.Animal.Id), objectIds);
        }

        #region Vector Search Filtering
        private async Task<List<(String id, double score)>> PerformVectorSearchAsync()
        {
            // Generate embedding for search query
            String cleanedQuery = base.CleanQuery();

            double[] queryEmbedding = (await _embeddingService.GenerateEmbeddingAsyncDouble(cleanedQuery)).Vector.ToArray();

            int pageSize = Math.Min(base.PageSize, _config.IndexSettings.Topk);
            int skip = base.Offset * pageSize;

            int totalNeeded = skip + pageSize;
            int vectorSearchLimit = Math.Max(totalNeeded, _config.IndexSettings.Topk);
            // MongoDB Vector Search aggregation pipeline - only return IDs and scores
            BsonDocument[] pipeline = new[]
            {
                new BsonDocument("$vectorSearch", new BsonDocument
                {
                    { "index", _config.IndexSettings.AnimalVectorSearchIndexName },
                    { "path", nameof(Data.Entities.Animal.Embedding) },
                    { "queryVector", new BsonArray(queryEmbedding) },
                    { "numCandidates", _config.IndexSettings.NumCandidates },
                    { "limit", vectorSearchLimit }
                }),
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "vectorScore", new BsonDocument("$meta", "vectorSearchScore") }
                }),
                new BsonDocument("$match", new BsonDocument
                {
                    { "vectorScore", new BsonDocument("$gte", _config.IndexSettings.VectorScoreThreshold) }
                }),
                new BsonDocument("$sort", new BsonDocument
                {
                    { "vectorScore", -1 }
                }),
                new BsonDocument("$skip", skip),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "vectorScore", 1 }
                })
            };

            // Apply internal collection use without the context session because thats how aggregation needs to work
            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>) _collection;
            List<BsonDocument> results = await scopedCollection.InternalCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.Select(doc =>
            {
                String id = doc["_id"].AsObjectId.ToString();
                double score = doc.GetValue("vectorScore", 0.0).AsDouble;

                return (id, score);
            }).ToList();
        }

        #endregion

        #region Schemantic Search Filtering

        private async Task<List<(String id, double score)>> PerformTextSearchAsync()
        {
            String cleanedQuery = base.CleanQuery();

            if (String.IsNullOrWhiteSpace(cleanedQuery)) return new List<(String id, double score)>();

            // Build comprehensive semantic search queries using config synonyms
            List<BsonDocument> searchQueries = await this.BuildSemanticSearchQueries(cleanedQuery);

            if (!searchQueries.Any()) return new List<(String id, double score)>();

            // Create the search stage with compound queries
            BsonDocument searchStage = new BsonDocument("$search", new BsonDocument
            {
                { "index", _config.IndexSettings.AnimalSchemanticIndexName },
                { "compound", new BsonDocument
                    {
                        { "should", new BsonArray(searchQueries) },
                        { "minimumShouldMatch", 1 }
                    }
                }
            });

            int pageSize = Math.Min(base.PageSize, _config.IndexSettings.Topk);
            int skip = base.Offset * pageSize;
            BsonDocument[] pipeline = new[]
            {
                searchStage,
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "textScore", new BsonDocument("$meta", "searchScore") }
                }),
                // Apply text score threshold from config
                new BsonDocument("$match", new BsonDocument
                {
                    { "textScore", new BsonDocument("$gte", _config.IndexSettings.TextScoreThreshold) }
                }),
                new BsonDocument("$sort", new BsonDocument
                {
                    { "textScore", -1 }
                }),
                new BsonDocument("$skip", skip),
                new BsonDocument("$limit", pageSize),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 1 },
                    { "textScore", 1 }
                }),
            };

            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;

            List<BsonDocument> results = await scopedCollection.InternalCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();

            return results.Select(doc =>
            {
                String id = doc["_id"].AsObjectId.ToString();
                double score = doc.GetValue("textScore", 0.0).AsDouble;

                return (id, score);
            }).ToList();
        }

        private async Task<List<BsonDocument>> BuildSemanticSearchQueries(String query)
        {
            List<BsonDocument> searchQueries = new List<BsonDocument>();

            // Primary semantic search on description with highest boost
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray {
                    "description",
                    "description.english",
                    "description.standard",
                    "description.stemmed"
                } },
                { "fuzzy", new BsonDocument
                    {
                        { "maxEdits", 2 },
                        { "prefixLength", 1 }
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", 4.0)) }
            }));

            // Name search with corrected field paths
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray {
                    "name",
                    "name.exact",
                    "name.english"
                } },
                { "fuzzy", new BsonDocument { { "maxEdits", 1 } } },
                { "score", new BsonDocument("boost", new BsonDocument("value", 3.0)) }
            }));

            // Separate autocomplete search for name (uses separate field)
            searchQueries.Add(new BsonDocument("autocomplete", new BsonDocument
            {
                { "query", query },
                { "path", "nameAutocomplete" },
                { "score", new BsonDocument("boost", new BsonDocument("value", 2.8)) }
            }));

            // Separate autocomplete search for description
            searchQueries.Add(new BsonDocument("autocomplete", new BsonDocument
            {
                { "query", query },
                { "path", "descriptionAutocomplete" },
                { "score", new BsonDocument("boost", new BsonDocument("value", 2.5)) }
            }));

            // Healthstatus semantic search
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray {
                    "healthStatus",
                    "healthStatus.exact",
                    "healthStatus.standard"
                } },
                { "fuzzy", new BsonDocument { { "maxEdits", 2 } } },
                { "score", new BsonDocument("boost", new BsonDocument("value", 2.0)) }
            }));

            // Numeric searches
            this.AddNumericSearchQueries(searchQueries, query);

            // Gender searches (fixed case sensitivity)
            this.AddGenderSearchQueries(searchQueries, query);

            // Config-based synonym searches
            this.AddConfigBasedSynonymSearches(searchQueries, query);

            return await Task.FromResult(searchQueries);
        }

        private void AddNumericSearchQueries(List<BsonDocument> searchQueries, String query)
        {
            if (double.TryParse(query, out double numericValue))
            {
                // Intelligent age range based on value
                double ageRange = numericValue switch
                {
                    < 1 => 0.5,      // Very young animals
                    < 5 => 1.0,      // Young animals
                    < 10 => 2.0,     // Adult animals
                    _ => 3.0         // Senior animals
                };

                searchQueries.Add(new BsonDocument("range", new BsonDocument
                {
                    { "path", "age" },
                    { "gte", Math.Max(0, numericValue - ageRange) },
                    { "lte", numericValue + ageRange },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 2.5)) }
                }));

                // Intelligent weight range based on value
                double weightRange = numericValue switch
                {
                    < 5 => 2.0,      // Very small animals
                    < 20 => 5.0,     // Small to medium animals
                    < 50 => 10.0,    // Medium to large animals
                    _ => 20.0        // Very large animals
                };

                searchQueries.Add(new BsonDocument("range", new BsonDocument
                {
                    { "path", "weight" },
                    { "gte", Math.Max(0, numericValue - weightRange) },
                    { "lte", numericValue + weightRange },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 2.0)) }
                }));
            }
        }

        private void AddGenderSearchQueries(List<BsonDocument> searchQueries, String lowerQuery)
        {
            // Dynamic gender mapping based on the actual enum values
            Dictionary<String, Gender[]> genderMappings = new Dictionary<String, Gender[]>
            {
                { "male", new[] { Gender.Male } },
                { "female", new[] { Gender.Female } },
                { "boy", new[] { Gender.Male } },
                { "girl", new[] { Gender.Female } },
                { "tom", new[] { Gender.Male } },         // Male cat
                { "queen", new[] { Gender.Female } },     // Female cat
                { "buck", new[] { Gender.Male } },        // Male rabbit
                { "doe", new[] { Gender.Female } },       // Female rabbit
                { "stallion", new[] { Gender.Male } },    // Male horse
                { "mare", new[] { Gender.Female } },      // Female horse
                { "cock", new[] { Gender.Male } },        // Male bird
                { "hen", new[] { Gender.Female } },       // Female bird
                { "masculine", new[] { Gender.Male } },
                { "feminine", new[] { Gender.Female } }
            };

            foreach (KeyValuePair<String, Gender[]> mapping in genderMappings)
            {
                if (lowerQuery.Contains(mapping.Key))
                {
                    foreach (Gender gender in mapping.Value)
                    {
                        // Use both numeric value and String representation for better matching
                        searchQueries.Add(new BsonDocument("text", new BsonDocument
                        {
                            { "query", gender.ToString() },
                            { "path", "gender" },
                            { "score", new BsonDocument("boost", new BsonDocument("value", 3.5)) }
                        }));

                        // Also search by numeric value in case it's stored as number
                        searchQueries.Add(new BsonDocument("equals", new BsonDocument
                        {
                            { "path", "gender" },
                            { "value", (int)gender },
                            { "score", new BsonDocument("boost", new BsonDocument("value", 3.5)) }
                        }));
                    }

                    break;
                }
            }
        }

        private void AddConfigBasedSynonymSearches(List<BsonDocument> searchQueries, String query)
        {

            HashSet<String> processedSynonyms = new HashSet<String>(); // Avoid duplicates

            // Process ALL categories, don't exit early
            foreach (IndexSynonyms categoryGroup in _config.IndexSettings.SynonymsBatch)
            {
                foreach (String synonymGroup in categoryGroup.Synonyms)
                {
                    String[] synonyms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    // Check if query matches any synonym in this group
                    if (synonyms.Any(synonym => query.Contains(synonym) || this.IsPartialMatch(query, synonym)))
                    {
                        // Add searches for all synonyms in this group
                        foreach (String synonym in synonyms)
                        {
                            if (processedSynonyms.Add(synonym)) // Only add if not already processed
                            {
                                double boostValue = this.GetBoostValueForCategory(categoryGroup.Category);
                                String[] searchPaths = this.GetSearchPathsForCategory(categoryGroup.Category);

                                // Add main synonym search
                                searchQueries.Add(new BsonDocument("text", new BsonDocument
                                {
                                    { "query", synonym },
                                    { "path", new BsonArray(searchPaths) },
                                    { "fuzzy", new BsonDocument { { "maxEdits", 1 } } },
                                    { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                                }));

                                // Add exact match variant for important terms
                                if (this.IsHighValueTerm(synonym, categoryGroup.Category))
                                {
                                    searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                                {
                                    { "query", synonym },
                                    { "path", searchPaths[0] }, // Use primary path for exact matches
                                    { "score", new BsonDocument("boost", new BsonDocument("value", boostValue * 1.2)) }
                                }));
                                }
                            }
                        }
                        break; // Found match in this synonym group, move to next group
                    }
                }
            }

            return;
        }

        #endregion

        #region Combine Free-Text Results
        private List<String> CombineSearchResultsToIds(
               List<(String id, double score)> vectorResults,
               List<(String id, double score)> textResults)
        {
            // Handle edge cases
            if (!vectorResults.Any() && !textResults.Any()) return new List<String>();

            if (!vectorResults.Any()) return textResults.OrderByDescending(r => r.score).Select(r => r.id).ToList();

            if (!textResults.Any()) return vectorResults.OrderByDescending(r => r.score).Select(r => r.id).ToList();

            // Normalize scores to 0-1 range for fair combination
            double maxVectorScore = vectorResults.Max(r => r.score);
            double maxTextScore = textResults.Max(r => r.score);

            Dictionary<String, double> combinedScores = new Dictionary<String, double>();

            // Add vector results with 70% weight
            foreach ((String id, double score) in vectorResults)
            {
                double normalizedScore = maxVectorScore > 0 ? score / maxVectorScore : 0;
                combinedScores[id] = normalizedScore * 0.7;
            }

            // Add text results with 30% weight
            foreach ((String id, double score) in textResults)
            {
                double normalizedScore = maxTextScore > 0 ? score / maxTextScore : 0;

                if (combinedScores.ContainsKey(id))
                {
                    combinedScores[id] += normalizedScore * 0.3;
                }
                else
                {
                    combinedScores[id] = normalizedScore * 0.3;
                }
            }

            // Return IDs ordered by combined score (highest first)
            return combinedScores
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }
        #endregion

        private Boolean ContainsAllWords(String query, String[] words)
        {
            return words.All(word => query.Contains(word.ToLower()));
        }
        private Boolean IsPartialMatch(String query, String synonym)
        {
            if (synonym.Contains(" "))
            {
                var words = synonym.Split(' ');
                return words.Any(word => query.Contains(word) && word.Length > 2);
            }
            if (synonym.Length > 4)
            {
                return query.Contains(synonym.Substring(0, Math.Min(4, synonym.Length)));
            }

            return false;
        }

        private Boolean IsHighValueTerm(String synonym, String category)
        {
            return category.ToLower() switch
            {
                "dog_breeds" or "cat_breeds" => true,
                "animal_types" => true,
                "health_status" => synonym.Length > 4, // Only longer health terms
                "adoption_status" => true,
                _ => false
            };
        }

        private double GetBoostValueForCategory(String category)
        {
            return category.ToLower() switch
            {
                "dog_breeds" => 3.2,
                "cat_breeds" => 3.2,
                "animal_types" => 3.8,
                "personalities" => 2.5,
                "health_status" => 2.8,
                "training_behavior" => 2.3,
                "colors_patterns" => 1.5,
                "coat_types" => 1.4,
                "sizes" => 1.8,
                "ages" => 2.2,
                "adoption_status" => 2.9,
                "special_needs" => 2.6,
                "living_situations" => 1.9,
                "gender_related" => 3.5,
                _ => 1.5 // Default boost for unknown categories
            };
        }

        private String[] GetSearchPathsForCategory(String category)
        {
            return category.ToLower() switch
            {
                "dog_breeds" or "cat_breeds" => new[] { "description", "name" },
                "animal_types" => new[] { "description", "name" },
                "personalities" => new[] { "description", "healthStatus" },
                "health_status" => new[] { "healthStatus", "description" },
                "training_behavior" => new[] { "description", "healthStatus" },
                "colors_patterns" => new[] { "description", "name" },
                "coat_types" => new[] { "description" },
                "sizes" => new[] { "description", "name" },
                "ages" => new[] { "description", "name" },
                "adoption_status" => new[] { "adoptionStatus", "description" },
                "special_needs" => new[] { "description", "healthStatus" },
                "living_situations" => new[] { "description" },
                "gender_related" => new[] { "gender", "description" },
                _ => new[] { "description", "name" } 
            };
        }

        #endregion

        public override async Task<FilterDefinition<Data.Entities.Animal>> ApplyAuthorization(FilterDefinition<Data.Entities.Animal> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimals))
                    return filter;
				else throw new ForbiddenException();

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any()) return new List<String>();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalDto στα ονόματα πεδίων Animal
				projectionFields.Add(nameof(Data.Entities.Animal.Id));
				if (item.Equals(nameof(Models.Animal.Animal.Name))) projectionFields.Add(nameof(Data.Entities.Animal.Name));
				if (item.Equals(nameof(Models.Animal.Animal.Description))) projectionFields.Add(nameof(Data.Entities.Animal.Description));
				if (item.Equals(nameof(Models.Animal.Animal.Gender))) projectionFields.Add(nameof(Data.Entities.Animal.Gender));
				if (item.Equals(nameof(Models.Animal.Animal.Age))) projectionFields.Add(nameof(Data.Entities.Animal.Age));
				if (item.Equals(nameof(Models.Animal.Animal.Weight))) projectionFields.Add(nameof(Data.Entities.Animal.Weight));
				if (item.Equals(nameof(Models.Animal.Animal.AdoptionStatus))) projectionFields.Add(nameof(Data.Entities.Animal.AdoptionStatus));
				if (item.Equals(nameof(Models.Animal.Animal.HealthStatus))) projectionFields.Add(nameof(Data.Entities.Animal.HealthStatus));
				if (item.Equals(nameof(Models.Animal.Animal.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Animal.CreatedAt));
				if (item.Equals(nameof(Models.Animal.Animal.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Animal.UpdatedAt));
                
				if (item.StartsWith(nameof(Models.Animal.Animal.AttachedPhotos))) projectionFields.Add(nameof(Data.Entities.Animal.PhotosIds));
                if (item.StartsWith(nameof(Models.Animal.Animal.Shelter))) projectionFields.Add(nameof(Data.Entities.Animal.ShelterId));
				if (item.StartsWith(nameof(Models.Animal.Animal.Breed))) projectionFields.Add(nameof(Data.Entities.Animal.BreedId));
				if (item.StartsWith(nameof(Models.Animal.Animal.AnimalType))) projectionFields.Add(nameof(Data.Entities.Animal.AnimalTypeId));
			}

			return projectionFields.ToList();
		}
	}
}