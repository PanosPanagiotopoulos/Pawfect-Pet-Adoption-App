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
using MongoDB.Bson.Serialization;
using Pawfect_Pet_Adoption_App_API.Services.TranslationServices;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Animals;
using System.Text.RegularExpressions;

namespace Main_API.Query.Queries
{
	public class AnimalQuery : BaseQuery<Data.Entities.Animal>
	{
        private readonly IEmbeddingService _embeddingService;
        private readonly ITranslationService _translationService;
        private readonly MongoDbConfig _config;

        public AnimalQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService authorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver authorizationContentResolver,
            IEmbeddingService _embeddingService,
            ITranslationService translationService,
            IOptions<MongoDbConfig> options

        ) : base(mongoDbService, authorizationService, authorizationContentResolver, claimsExtractor)
        {
            this._embeddingService = _embeddingService;
            this._translationService = translationService;
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
        public Double? AgeFrom { get; set; }
        public Double? AgeTo { get; set; }

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

            return await Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Animal>> ApplyAuthorization(FilterDefinition<Data.Entities.Animal> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimals))
                    return filter;
                else throw new ForbiddenException();

            return await Task.FromResult(filter);
        }

        #endregion

        public override async Task<List<Data.Entities.Animal>> CollectAsync()
        {
            // If no search query is set, use the base implementation
            if (String.IsNullOrWhiteSpace(this.Query))
            {
                return await base.CollectAsync();
            }

            FilterDefinition<Data.Entities.Animal> filter = await this.ApplyFilters();
            filter = await this.ApplyAuthorization(filter);

            RenderArgs<Data.Entities.Animal> renderArgs = new RenderArgs<Data.Entities.Animal>(
                BsonSerializer.SerializerRegistry.GetSerializer<Data.Entities.Animal>(),
                BsonSerializer.SerializerRegistry
            );

            BsonDocument filterDoc = filter.Render(renderArgs);

            String multilingualQuery = await _translationService.TranslateAsync(this.CleanQuery(), null, SupportedLanguages.English);

            Task<List<AnimalSearchResult>> vectorSearchTask = this.AnimalVectorSearch(filterDoc, multilingualQuery);
            Task<List<AnimalSearchResult>> semanticSearchTask = this.AnimalSemanticSearch(filterDoc, multilingualQuery);

            List<AnimalSearchResult>[] results = await Task.WhenAll(vectorSearchTask, semanticSearchTask);

            List<Data.Entities.Animal> combinedResults = this.CombineSearchResults(results[0], results[1]);

            return combinedResults;
        }

        #region Vector Search
        private async Task<List<AnimalSearchResult>> AnimalVectorSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<AnimalSearchResult>();

            Double[] queryEmbedding = (await _embeddingService.GenerateEmbeddingAsyncDouble(query)).Vector.ToArray();

            int requestedResults = this.PageSize;
            int searchLimit = Math.Min(
                Math.Max(requestedResults * 3, _config.IndexSettings.Topk), 
                1000 
            );

            int numCandidates = Math.Min(
                Math.Max(searchLimit * 10, _config.IndexSettings.NumCandidates), 
                10000 
            );

            // Build vector search aggregation pipeline
            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                // Vector search stage
                new BsonDocument("$vectorSearch", new BsonDocument
                {
                    { "index", _config.IndexSettings.AnimalVectorSearchIndexName },
                    { "path", nameof(Data.Entities.Animal.Embedding) },
                    { "queryVector", new BsonArray(queryEmbedding) },
                    { "numCandidates", _config.IndexSettings.NumCandidates },
                    { "limit", searchLimit },
                }),

                new BsonDocument("$addFields", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore), new BsonDocument("$meta", "vectorSearchScore") }
                }),

                //  Apply score threshold (use lower threshold for better recall)
                new BsonDocument("$match", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore), new BsonDocument("$gte", _config.IndexSettings.VectorScoreThreshold) }
                }),
            };

            // Add base filters if any
            if (baseFilter != null && baseFilter.ElementCount > 0)
            {
                pipeline.Add(new BsonDocument("$match", baseFilter));
            }

            // Vector search extra querying stuff
            base.Fields ??= new List<String>();
            base.Fields.Add(nameof(Data.Entities.Animal.SearchScore));

            base.SortBy ??= new List<String>();
            base.SortBy.Add(nameof(Data.Entities.Animal.SearchScore));
            base.SortDescending = true;

            // Apply sorting, pagination, and projection
            pipeline = this.ApplySorting(pipeline);
            pipeline = this.ApplyPagination(pipeline);
            pipeline = this.ApplyProjection(pipeline);

            // Execute aggregation
            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;

            // Use BsonDocument to capture both animal data and search score
            List<Data.Entities.Animal> rawResults = await scopedCollection.InternalCollection.Aggregate<Data.Entities.Animal>(pipeline).ToListAsync();

            List<AnimalSearchResult> results = rawResults.Select((doc, index) =>
            {
                return new AnimalSearchResult()
                {
                    Animal = doc,
                    VectorScore = doc.SearchScore ?? 0,
                    SemanticScore = 0,
                    CombinedScore = doc.SearchScore ?? 0,
                    VectorRank = index + 1,
                    SemanticRank = int.MaxValue
                };
            })
            .ToList();

            return results;
        }

        #endregion

        #region Schemantic Search Filtering
        private async Task<List<AnimalSearchResult>> AnimalSemanticSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<AnimalSearchResult>();

            // Build STRICT semantic search pipeline
            List<BsonDocument> pipeline = new List<BsonDocument>
            {   
                // Primary semantic search with stricter requirements
                new BsonDocument("$search", new BsonDocument
                {
                    { "index", _config.IndexSettings.AnimalSchemanticIndexName },
                    { "compound", new BsonDocument
                        {
                            { "should", new BsonArray(await BuildSemanticTextQueries(query)) },
                            // Increase minimum should match for multi-word queries
                            { "minimumShouldMatch", GetMinimumShouldMatch(query) }
                        }
                    }
                }),

                // Add search score for ranking
                new BsonDocument("$addFields", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore), new BsonDocument("$meta", "searchScore") }
                }),

                // Apply MUCH stricter threshold
                new BsonDocument("$match", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore), new BsonDocument("$gte", this.GetSemanticThreshold(query)) }
                })
            };

            // Add base filters if any
            if (baseFilter != null && baseFilter.ElementCount > 0)
            {
                pipeline.Add(new BsonDocument("$match", baseFilter));
            }

            // Semantic search extra querying stuff
            base.Fields ??= new List<String>();
            base.Fields.Add(nameof(Data.Entities.Animal.SearchScore));

            base.SortBy ??= new List<String>();
            base.SortBy.Add(nameof(Data.Entities.Animal.SearchScore));
            base.SortDescending = true;

            pipeline = this.ApplySorting(pipeline);
            pipeline = this.ApplyPagination(pipeline);
            pipeline = this.ApplyProjection(pipeline);

            // Execute aggregation
            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;
            List<Data.Entities.Animal> rawResults = await scopedCollection.InternalCollection.Aggregate<Data.Entities.Animal>(pipeline).ToListAsync();

            List<AnimalSearchResult> results = rawResults.Select((doc, index) =>
            {
                Double normalizedScore = this.NormalizeSearchScore(doc.SearchScore ?? 0, query);

                return new AnimalSearchResult()
                {
                    Animal = doc,
                    VectorScore = 0,
                    SemanticScore = normalizedScore,
                    CombinedScore = normalizedScore,
                    VectorRank = int.MaxValue,
                    SemanticRank = index + 1
                };
            })
            .ToList();

            return results;
        }
        private async Task<List<BsonDocument>> BuildSemanticTextQueries(String query)
        {
            List<BsonDocument> searchQueries = new List<BsonDocument>();

            // PHRASE search - highest priority for exact matches
           searchQueries.Add(new BsonDocument("phrase", new BsonDocument
            {
                { "query", query },
                { "path", nameof(Data.Entities.Animal.SemanticText) },
                { "slop", 3 },
                { "score", new BsonDocument("boost", new BsonDocument("value", 30.0)) }
            }));

            // MULTI-LANGUAGE TEXT search - covers all analyzers
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray
                    {
                        nameof(Data.Entities.Animal.SemanticText),
                        $"{nameof(Data.Entities.Animal.SemanticText)}.english",
                        $"{nameof(Data.Entities.Animal.SemanticText)}.greek"
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", 25.0)) }
            }));

            // SYNONYM search - critical for cross-language matching
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", nameof(Data.Entities.Animal.SemanticText) },
                { "synonyms", "pet_synonyms" },
                { "score", new BsonDocument("boost", new BsonDocument("value", 22.0)) }
            }));

            // DESCRIPTION search - multi-language
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray
                    {
                        nameof(Data.Entities.Animal.Description),
                        $"{nameof(Data.Entities.Animal.Description)}.english",
                        $"{nameof(Data.Entities.Animal.Description)}.greek"
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", 15.0)) }
            }));

            // FUZZY search - for typos (only if query is long enough)
            if (query.Length > 4)
            {
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", query },
                    { "path", nameof(Data.Entities.Animal.SemanticText) },
                    { "fuzzy", new BsonDocument
                        {
                            { "maxEdits", 1 },
                            { "prefixLength", 3 },
                            { "maxExpansions", 25 }
                        }
                    },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 10.0)) }
                }));
            }

            // HEALTH STATUS search - multi-language
            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray
                    {
                        nameof(Data.Entities.Animal.HealthStatus),
                        $"{nameof(Data.Entities.Animal.HealthStatus)}.english",
                        $"{nameof(Data.Entities.Animal.HealthStatus)}.greek"
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", 8.0)) }
            }));

            // AUTOCOMPLETE search - for partial matches
            searchQueries.Add(new BsonDocument("autocomplete", new BsonDocument
            {
                { "query", query },
                { "path", "semanticTextAutocomplete" },
                { "score", new BsonDocument("boost", new BsonDocument("value", 5.0)) }
            }));


            // Add gender-specific searches
            this.AddGenderSearchQueries(searchQueries, query);
            // Add numeric searches if needed
            this.AddNumericSearchQueries(searchQueries, query);

            return await Task.FromResult(searchQueries);
        }

        private void AddGenderSearchQueries(List<BsonDocument> searchQueries, String query)
        {
            // Define gender mappings for both languages
            // These now map to the enum String values that will be stored in MongoDB
            Dictionary<String, String> genderMappings = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
            {
                // English mappings to enum String value
                { "male", "Male" },
                { "boy", "Male" },
                { "he", "Male" },
                { "him", "Male" },
                { "masculine", "Male" },
                { "man", "Male" },
                { "female", "Female" },
                { "girl", "Female" },
                { "she", "Female" },
                { "her", "Female" },
                { "feminine", "Female" },
                { "woman", "Female" },
                // Greek mappings to enum String value
                { "αρσενικό", "Male" },
                { "αρσενικος", "Male" },
                { "αρσενικός", "Male" },
                { "αγόρι", "Male" },
                { "αρρεν", "Male" },
                { "άντρας", "Male" },
                { "θηλυκό", "Female" },
                { "θηλυκη", "Female" },
                { "θηλυκή", "Female" },
                { "κορίτσι", "Female" },
                { "θηλυ", "Female" },
                { "γυναίκα", "Female" }
            };

            // Check if query contains gender terms
            Boolean hasGenderTerm = false;
            String detectedGender = null;

            foreach (KeyValuePair<String, String> mapping in genderMappings)
            {
                if (query.Contains(mapping.Key.ToLower()))
                {
                    hasGenderTerm = true;
                    detectedGender = mapping.Value;
                    break;
                }
            }

            if (hasGenderTerm && detectedGender != null)
            {
                // EXACT match for Gender field (highest priority)
                // Since Gender is now stored as String, we can search it directly
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", detectedGender },
                    { "path", nameof(Data.Entities.Animal.Gender) },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 20.0)) }
                }));

                // Add phrase search for exact gender match
                searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                {
                    { "query", detectedGender },
                    { "path", nameof(Data.Entities.Animal.Gender) },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 18.0)) }
                }));

                // Multi-language text search on Gender field
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", query },
                    { "path", new BsonArray
                        {
                            nameof(Data.Entities.Animal.Gender),
                            $"{nameof(Data.Entities.Animal.Gender)}.english",
                            $"{nameof(Data.Entities.Animal.Gender)}.greek"
                        }
                    },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 15.0)) }
                }));
            }
            else
            {
                // If no specific gender term detected, still search gender field with lower boost
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", query },
                    { "path", new BsonArray
                        {
                            nameof(Data.Entities.Animal.Gender),
                            $"{nameof(Data.Entities.Animal.Gender)}.english",
                            $"{nameof(Data.Entities.Animal.Gender)}.greek"
                        }
                    },
                    { "score", new BsonDocument("boost", new BsonDocument("value", 9.0)) }
                }));
            }
        }

        private void AddNumericSearchQueries(List<BsonDocument> searchQueries, String query)
        {
            // Age patterns in both languages
            String[] agePatterns = new[]
            {
                @"(\d+)\s*(year|χρον|ετ)", // English/Greek year
                @"(\d+)\s*(month|μην)", // English/Greek month  
                @"(\d+)\s*(yr|ετ)", // Abbreviations
                @"(age|ηλικ)\s*[:=]?\s*(\d+)" // Age indicators
            };

            foreach (String pattern in agePatterns)
            {
                Match match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out Double age))
                    {
                        // Convert months to years if needed
                        if (pattern.Contains("month") || pattern.Contains("μην"))
                        {
                            age = age / 12.0;
                        }

                        searchQueries.Add(new BsonDocument("range", new BsonDocument
                        {
                            { "path", nameof(Data.Entities.Animal.Age) },
                            { "gte", Math.Max(0, age - 0.5) },
                            { "lte", age + 0.5 },
                            { "score", new BsonDocument("boost", new BsonDocument("value", 12.0)) }
                        }));
                        break;
                    }
                }
            }

            // Weight patterns in both languages
            String[] weightPatterns = new[]
            {
                @"(\d+)\s*(kg|kilo|κιλ)", // Kilograms
                @"(\d+)\s*(lb|pound|λίμπρα)", // Pounds
                @"(weight|βάρος)\s*[:=]?\s*(\d+)" // Weight indicators
            };

            foreach (String pattern in weightPatterns)
            {
                Match match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out Double weight))
                    {
                        // Convert pounds to kg if needed
                        if (pattern.Contains("lb") || pattern.Contains("pound") || pattern.Contains("λίμπρα"))
                        {
                            weight = weight * 0.453592;
                        }

                        searchQueries.Add(new BsonDocument("range", new BsonDocument
                        {
                            { "path", nameof(Data.Entities.Animal.Weight) },
                            { "gte", Math.Max(0, weight - 1.0) },
                            { "lte", weight + 1.0 },
                            { "score", new BsonDocument("boost", new BsonDocument("value", 10.0)) }
                        }));
                        break;
                    }
                }
            }
        }
        #endregion

        #region Combine Results
        private List<Data.Entities.Animal> CombineSearchResults(
             List<AnimalSearchResult> vectorResults,
             List<AnimalSearchResult> semanticResults)
        {
            if (!vectorResults.Any() && !semanticResults.Any())
            {
                return new List<Data.Entities.Animal>();
            }

            if (!vectorResults.Any())
            {
                return semanticResults
                    .OrderByDescending(x => x.SemanticScore)
                    .Take(this.PageSize)
                    .Select(x => x.Animal)
                    .ToList();
            }

            if (!semanticResults.Any())
            {
                return vectorResults
                    .OrderByDescending(x => x.VectorScore)
                    .Take(this.PageSize)
                    .Select(x => x.Animal)
                    .ToList();
            }

            SearchCombinationSettings combinationSettings = _config.IndexSettings.SearchSettings?.CombinationSettings;
            Double vectorWeight = combinationSettings?.VectorWeight ?? 0.45;
            Double semanticWeight = combinationSettings?.SemanticWeight ?? 0.55;

            // Use Dictionary to combine and avoid duplicates
            Dictionary<String, AnimalSearchResult> combinedMap = new Dictionary<String, AnimalSearchResult>();

            // Process semantic results first (higher priority)
            foreach (AnimalSearchResult semanticResult in semanticResults)
            {
                String id = semanticResult.Animal.Id.ToString();

                combinedMap[id] = new AnimalSearchResult
                {
                    Animal = semanticResult.Animal,
                    VectorScore = 0,
                    SemanticScore = semanticResult.SemanticScore,
                    CombinedScore = semanticResult.SemanticScore * semanticWeight,
                    VectorRank = int.MaxValue,
                    SemanticRank = semanticResult.SemanticRank
                };
            }

            // Process vector results and merge with semantic if exists
            foreach (AnimalSearchResult vectorResult in vectorResults)
            {
                String id = vectorResult.Animal.Id.ToString();

                if (combinedMap.ContainsKey(id))
                {
                    // Animal found in both searches - enhance existing semantic result
                    AnimalSearchResult existing = combinedMap[id];
                    existing.VectorScore = vectorResult.VectorScore;
                    existing.VectorRank = vectorResult.VectorRank;

                    // Bonus for appearing in both searches with semantic priority boost
                    Double bonusMultiplier = 1.4; // Higher bonus to prioritize dual matches
                    existing.CombinedScore = (vectorResult.VectorScore * vectorWeight +
                                             existing.SemanticScore * semanticWeight) * bonusMultiplier;
                }
                else
                {
                    // Only add vector-only results if we have space
                    combinedMap[id] = new AnimalSearchResult
                    {
                        Animal = vectorResult.Animal,
                        VectorScore = vectorResult.VectorScore,
                        SemanticScore = 0,
                        CombinedScore = vectorResult.VectorScore * vectorWeight,
                        VectorRank = vectorResult.VectorRank,
                        SemanticRank = int.MaxValue
                    };
                }
            }

            // Sort results with semantic search priority
            List<AnimalSearchResult> sortedResults = combinedMap.Values
                .OrderByDescending(x => x.SemanticScore > 0 ? 1 : 0) // Semantic results first
                .ThenByDescending(x => x.CombinedScore) // Then by combined score
                .ThenByDescending(x => Math.Max(x.VectorScore, x.SemanticScore)) // Then by highest individual score
                .Take(this.PageSize) // Apply page size limit
                .ToList();

            return sortedResults.Select(x => x.Animal).ToList();
        }
        #endregion

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        public override List<String> FieldNamesOf(List<String> fields)
        {
            if (fields == null || !fields.Any()) return new List<String>();

            HashSet<String> projectionFields = new HashSet<String>();
            foreach (String item in fields)
            {
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
                
                // For search related
                if (item.Equals(nameof(Data.Entities.Animal.Embedding))) projectionFields.Add(nameof(Data.Entities.Animal.Embedding));
                if (item.Equals(nameof(Data.Entities.Animal.SemanticText))) projectionFields.Add(nameof(Data.Entities.Animal.SemanticText));

                if (item.StartsWith(nameof(Models.Animal.Animal.AttachedPhotos))) projectionFields.Add(nameof(Data.Entities.Animal.PhotosIds));
                if (item.StartsWith(nameof(Models.Animal.Animal.Shelter))) projectionFields.Add(nameof(Data.Entities.Animal.ShelterId));
                if (item.StartsWith(nameof(Models.Animal.Animal.Breed))) projectionFields.Add(nameof(Data.Entities.Animal.BreedId));
                if (item.StartsWith(nameof(Models.Animal.Animal.AnimalType))) projectionFields.Add(nameof(Data.Entities.Animal.AnimalTypeId));
            }

            return projectionFields.ToList();
        }

        #region Helpers
        private int GetMinimumShouldMatch(String query)
        {
            int wordCount = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            // For multi-word queries, require more matches
            if (wordCount >= 4)
                return 3; // At least 3 clauses must match
            else if (wordCount >= 2)
                return 2; // At least 2 clauses must match
            else
                return 1; // Single word queries need at least 1 match
        }

        private Double GetSemanticThreshold(String query)
        {
            // MUCH stricter thresholds for accuracy
            int wordCount = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            // With base threshold of 0.9
            if (wordCount <= 1)
            {
                // Single word searches - needs strong match
                return _config.IndexSettings.TextScoreThreshold * 3.0; // 2.7
            }
            else if (wordCount <= 2)
            {
                // Two word searches - require very strong match
                return _config.IndexSettings.TextScoreThreshold * 4.0; // 3.6
            }
            else if (wordCount <= 4)
            {
                // Normal searches - strict
                return _config.IndexSettings.TextScoreThreshold * 5.0; // 4.5
            }
            else
            {
                // Long queries - extremely strict
                return _config.IndexSettings.TextScoreThreshold * 6.0; // 5.4
            }
        }

        private Double NormalizeSearchScore(Double rawScore, String query)
        {
            // Normalize scores to 0-1 range based on expected ranges
            // Adjust max expected score based on your testing
            Double maxExpectedScore = 100.0; // Increased for stricter matching
            Double normalizedScore = rawScore / maxExpectedScore;

            // Apply sigmoid for better distribution
            return 1.0 / (1.0 + Math.Exp(-5 * (normalizedScore - 0.5)));
        }

        #endregion
    }
}