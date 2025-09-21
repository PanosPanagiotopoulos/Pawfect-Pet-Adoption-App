using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.MongoServices;
using Pawfect_API.Services.EmbeddingServices;
using Pawfect_API.Data.Entities.Types.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using Pawfect_API.Services.TranslationServices;
using Pawfect_API.Data.Entities.Types.Translation;
using Pawfect_API.Data.Entities.Types.Animals;
using System.Text.RegularExpressions;
using Pawfect_API.Data.Entities.Types.Search;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Embedding;

namespace Pawfect_API.Query.Queries
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
		public List<String>? AnimalTypeIds { get; set; }

		// Λίστα από καταστάσεις υιοθεσίας για φιλτράρισμα
		public List<AdoptionStatus>? AdoptionStatuses { get; set; }
        public List<Gender>? Genders { get; set; }
        public Double? AgeFrom { get; set; }
        public Double? AgeTo { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        public Boolean? UseVectorSearch { get; set; }

        public Boolean? UseSemanticSearch { get; set; }

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
			if (AnimalTypeIds != null && AnimalTypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = AnimalTypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

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

            Task<List<AnimalSearchResult>> vectorSearchTask = Task.FromResult(new List<AnimalSearchResult>());
            if (this.UseVectorSearch.GetValueOrDefault(false))
                vectorSearchTask = Task.FromResult(new List<AnimalSearchResult>());

            Task<List<AnimalSearchResult>> semanticSearchTask = Task.FromResult(new List<AnimalSearchResult>());
            if (this.UseSemanticSearch.GetValueOrDefault(false))
                 semanticSearchTask = this.AnimalSemanticSearch(filterDoc, multilingualQuery);

            List<AnimalSearchResult>[] results = await Task.WhenAll(vectorSearchTask, semanticSearchTask);

            List<Data.Entities.Animal> combinedResults = this.CombineSearchResults(results[0], results[1]);

            return combinedResults;
        }

        #region Vector Search
        private async Task<List<AnimalSearchResult>> AnimalVectorSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<AnimalSearchResult>();

            Double[] queryEmbedding = (await _embeddingService.GenerateAggregatedEmbeddingAsyncDouble(
                new ChunkedEmbeddingInput<String>()
                {
                    Content = query,
                    SourceId = null,
                    SourceType = nameof(String)
                }
            )).Vector.ToArray();

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

        #region Semantic Search
        private async Task<List<AnimalSearchResult>> AnimalSemanticSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<AnimalSearchResult>();

            QueryAnalysis queryAnalysis = this.AnalyzeQuery(query);

            List<BsonDocument> queries = await BuildSemanticTextQueries(query, queryAnalysis);
            int minimumShouldMatch = Math.Min(this.GetDynamicMinimumShouldMatch(query, queryAnalysis), queries.Count);

            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                new BsonDocument("$search", new BsonDocument
                {
                    { "index", _config.IndexSettings.AnimalSchemanticIndexName },
                    { "compound", new BsonDocument
                        {
                            { "should", new BsonArray(queries) },
                            { "minimumShouldMatch", minimumShouldMatch }
                        }
                    }
                }),

                new BsonDocument("$addFields", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore), new BsonDocument("$meta", "searchScore") },
                    { "QueryMatchType", queryAnalysis.MatchType.ToString() }
                }),

                new BsonDocument("$match", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.SearchScore),
                      new BsonDocument("$gte", this.GetDynamicSemanticThreshold(query, queryAnalysis)) }
                }),

                // Calculate AdjustedScore using nameof for the field name
                new BsonDocument("$addFields", new BsonDocument
                {
                    { nameof(Data.Entities.Animal.AdjustedScore), new BsonDocument
                        {
                            { "$multiply", new BsonArray
                                {
                                    "$" + nameof(Data.Entities.Animal.SearchScore),
                                    queryAnalysis.ScoreMultiplier
                                }
                            }
                        }
                    }
                })
            };

            if (baseFilter != null && baseFilter.ElementCount > 0)
            {
                pipeline.Add(new BsonDocument("$match", baseFilter));
            }

            base.Fields ??= new List<String>();
            base.Fields.Add(nameof(Data.Entities.Animal.SearchScore));
            base.Fields.Add(nameof(Data.Entities.Animal.AdjustedScore));

            base.SortBy ??= new List<String>();
            base.SortBy.Clear();
            base.SortBy.Add(nameof(Data.Entities.Animal.AdjustedScore)); 
            base.SortDescending = true;

            pipeline = this.ApplySorting(pipeline);
            pipeline = this.ApplyPagination(pipeline);
            pipeline = this.ApplyProjection(pipeline);

            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;
            List<Data.Entities.Animal> rawResults = await scopedCollection.InternalCollection.Aggregate<Data.Entities.Animal>(pipeline).ToListAsync();

            List<AnimalSearchResult> results = rawResults.Select((doc, index) =>
            {
                // Use the AdjustedScore (multiplied score) as the final semantic score
                Double finalScore = doc.AdjustedScore ?? doc.SearchScore ?? 0;
                Double normalizedScore = this.NormalizeSemanticScore(finalScore, query, queryAnalysis);

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

        private async Task<List<BsonDocument>> BuildSemanticTextQueries(String query, QueryAnalysis queryAnalysis)
        {
            List<BsonDocument> searchQueries = new List<BsonDocument>();
            BoostSettings boostSettings = _config.IndexSettings.SearchSettings?.BoostSettings ?? new BoostSettings();

            // More restrictive exact phrase search
            if (queryAnalysis.HasExactPhraseIntent)
            {
                searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                {
                    { "query", query },
                    { "path", nameof(Data.Entities.Animal.SemanticText) },
                    { "slop", 0 },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.ExactPhraseBoost)) }
                }));
            }
            else
            {
                // Reduced phrase slop for better precision
                searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                {
                    { "query", query },
                    { "path", nameof(Data.Entities.Animal.SemanticText) },
                    { "slop", Math.Min(queryAnalysis.PhraseSlop, 1) }, // Maximum slop of 1
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.PhraseWithSlopBoost)) }
                }));
            }

            // Standard text search without synonyms for better precision
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
                { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.SemanticTextBoost)) }
            }));

            // Only add synonyms for longer descriptive queries
            if (queryAnalysis.MatchType == SearchMatchType.Mixed || queryAnalysis.NeedsSynonymExpansion && queryAnalysis.MatchType == SearchMatchType.Descriptive)
            {
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", query },
                    { "path", nameof(Data.Entities.Animal.SemanticText) },
                    { "synonyms", "pet_synonyms" },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.SynonymBoost)) }
                }));
            }

            // Description search with lower boost
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
                { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.DescriptionBoost)) }
            }));

            FuzzySettings fuzzySettings = _config.IndexSettings.SearchSettings?.FuzzySettings ?? new FuzzySettings();
            if (queryAnalysis.MatchType == SearchMatchType.Mixed ||
                query.Length > fuzzySettings.MinQueryLength &&
                queryAnalysis.AllowFuzzySearch &&
                queryAnalysis.MatchType == SearchMatchType.Descriptive) 
            {
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", query },
                    { "path", nameof(Data.Entities.Animal.SemanticText) },
                    { "fuzzy", new BsonDocument
                        {
                            { "maxEdits", 1 }, 
                            { "prefixLength", Math.Min(6, query.Length - 1) }, 
                            { "maxExpansions", 3 } 
                        }
                    },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.FuzzySearchBoost)) }
                }));
            }

            // Health terms search
            if (queryAnalysis.HasHealthTerms)
            {
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
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.HealthTermsBoost)) }
                }));
            }

            // Gender-specific searches - ALWAYS add if gender terms detected
            if (queryAnalysis.HasGenderTerms)
            {
                this.AddGenderSearchQueries(searchQueries, query, queryAnalysis, boostSettings);
            }

            // Numeric searches
            if (queryAnalysis.HasNumericTerms)
            {
                this.AddNumericSearchQueries(searchQueries, query, queryAnalysis, boostSettings);
            }

            return await Task.FromResult(searchQueries);
        }

        private QueryAnalysis AnalyzeQuery(String query)
        {
            QueryAnalysis analysis = new QueryAnalysis();
            String[] words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Basic analysis
            analysis.HasExactPhraseIntent = query.Contains("\"") || words.Length <= 2;

            // Content analysis using configuration - FIXED: case insensitive
            analysis.HasHealthTerms = this.ContainsTermsFromCategory("health_status", query.ToLower());
            analysis.HasGenderTerms = this.ContainsTermsFromCategory("gender_related", query.ToLower());
            analysis.HasNumericTerms = Regex.IsMatch(query, @"\d+") ||
                this.ContainsAgeKeywords(query) ||
                this.ContainsWeightKeywords(query);

            analysis.IsPartialQuery = words.Length == 1 && query.Length < 4;

            // FIXED: Stricter logic for synonym expansion
            if (analysis.HasExactPhraseIntent)
            {
                analysis.MatchType = SearchMatchType.Exact;
                analysis.PhraseSlop = 0;
                analysis.ScoreMultiplier = 1.5;
                analysis.AllowFuzzySearch = false;
                analysis.NeedsSynonymExpansion = false; 
            }
            else if (words.Length <= 3 && !analysis.HasNumericTerms)
            {
                analysis.MatchType = SearchMatchType.Phrase;
                analysis.PhraseSlop = 1;
                analysis.ScoreMultiplier = 1.2;
                analysis.AllowFuzzySearch = false; 
                analysis.NeedsSynonymExpansion = false; 
            }
            else if (words.Length > 5)
            {
                analysis.MatchType = SearchMatchType.Descriptive;
                analysis.PhraseSlop = 3; 
                analysis.ScoreMultiplier = 0.8;
                analysis.AllowFuzzySearch = true;
                analysis.NeedsSynonymExpansion = true;
            }
            else
            {
                analysis.MatchType = SearchMatchType.Mixed;
                analysis.PhraseSlop = 2; 
                analysis.ScoreMultiplier = 1.0;
                analysis.AllowFuzzySearch = false; 
                analysis.NeedsSynonymExpansion = false; 
            }

            // Extract gender if present
            if (analysis.HasGenderTerms)
            {
                analysis.DetectedGender = this.ExtractGenderFromQuery(query);
            }

            return analysis;
        }

        private void AddGenderSearchQueries(List<BsonDocument> searchQueries, String query, QueryAnalysis queryAnalysis, BoostSettings boostSettings)
        {
            if (!String.IsNullOrEmpty(queryAnalysis.DetectedGender))
            {
                // Exact gender match
                searchQueries.Add(new BsonDocument("text", new BsonDocument
                {
                    { "query", queryAnalysis.DetectedGender },
                    { "path", nameof(Data.Entities.Animal.Gender) },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.GenderExactBoost)) }
                }));

                // Phrase gender match
                searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                {
                    { "query", queryAnalysis.DetectedGender },
                    { "path", nameof(Data.Entities.Animal.Gender) },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.GenderPhraseBoost)) }
                }));
            }

            // Multi-language gender search
            Double genderBoost = queryAnalysis.DetectedGender != null ? boostSettings.GenderExactBoost * 0.7 : boostSettings.GenderExactBoost * 0.4;
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
                { "score", new BsonDocument("boost", new BsonDocument("value", genderBoost)) }
             }));
        }

        private void AddNumericSearchQueries(List<BsonDocument> searchQueries, String query, QueryAnalysis queryAnalysis, BoostSettings boostSettings)
        {
            NumericPatternSettings numericSettings = _config.IndexSettings.SearchSettings?.NumericPatternSettings ?? new NumericPatternSettings();

            // Age patterns from configuration
            List<String> agePatterns = numericSettings.AgePatterns;
            List<String> monthKeywords = numericSettings.MonthKeywords;
            Double monthToYearConversion = numericSettings.MonthToYearConversion > 0 ? numericSettings.MonthToYearConversion : 0.0833333;

            foreach (String pattern in agePatterns)
            {
                Match match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    String numberGroup = GetNumberFromMatch(match);

                    if (Double.TryParse(numberGroup, out Double age))
                    {
                        // Check if this is a month-based pattern
                        if (IsMonthPattern(match.Value, monthKeywords))
                        {
                            age = age * monthToYearConversion;
                        }

                        Double tolerance = this.GetAgeTolerance(age);
                        searchQueries.Add(new BsonDocument("range", new BsonDocument
                        {
                            { "path", nameof(Data.Entities.Animal.Age) },
                            { "gte", Math.Max(0, age - tolerance) },
                            { "lte", age + tolerance },
                            { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.NumericRangeBoost)) }
                        }));
                        break;
                    }
                }
            }

            // Weight patterns from configuration
            List<String> weightPatterns = numericSettings.WeightPatterns;
            List<String> poundKeywords = numericSettings.PoundKeywords;
            Double poundToKgConversion = numericSettings.PoundToKgConversion > 0 ? numericSettings.PoundToKgConversion : 0.453592;

            foreach (String pattern in weightPatterns)
            {
                Match match = Regex.Match(query, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    String numberGroup = GetNumberFromMatch(match);

                    if (Double.TryParse(numberGroup, out Double weight))
                    {
                        // Check if this is a pound-based pattern
                        if (IsPoundPattern(match.Value, poundKeywords))
                        {
                            weight = weight * poundToKgConversion;
                        }

                        Double tolerance = this.GetWeightTolerance(weight);
                        searchQueries.Add(new BsonDocument("range", new BsonDocument
                {
                    { "path", nameof(Data.Entities.Animal.Weight) },
                    { "gte", Math.Max(0, weight - tolerance) },
                    { "lte", weight + tolerance },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostSettings.WeightRangeBoost)) }
                }));
                        break;
                    }
                }
            }
        }
        private Boolean ContainsAgeKeywords(String query)
        {
            if (_config.IndexSettings.MultilingualSettings?.AgeKeywords != null)
            {
                return _config.IndexSettings.MultilingualSettings.AgeKeywords
                    .Any(k => query.ToLower().Contains(k.Keyword.ToLower()));
            }

            KeywordSettings keywordSettings = _config.IndexSettings.SearchSettings?.KeywordSettings ?? new KeywordSettings();
            List<String> ageKeywords = keywordSettings.AgeKeywords;

            String lowerQuery = query.ToLower();
            return ageKeywords.Any(keyword => lowerQuery.Contains(keyword.ToLower()));
        }

        private Boolean ContainsWeightKeywords(String query)
        {
            KeywordSettings keywordSettings = _config.IndexSettings.SearchSettings?.KeywordSettings ?? new KeywordSettings();
            List<String> weightKeywords = keywordSettings.WeightKeywords;

            String lowerQuery = query.ToLower();
            return weightKeywords.Any(keyword => lowerQuery.Contains(keyword.ToLower()));
        }

        private Boolean ContainsTermsFromCategory(String category, String query)
        {
            // Check both English and Greek synonym batches
            IEnumerable<IndexSynonyms> allBatches = (_config.IndexSettings.SynonymsBatch ?? new List<IndexSynonyms>())
                .Concat(_config.IndexSettings.GreekSynonymsBatch ?? new List<IndexSynonyms>());

            String[] targetCategories = new[] { category, $"{category}_greek" };

            foreach (IndexSynonyms batch in allBatches)
            {
                if (targetCategories.Contains(batch.Category))
                {
                    foreach (String synonymGroup in batch.Synonyms)
                    {
                        String[] terms = synonymGroup.Split(',');
                        if (terms.Any(term => query.Contains(term.Trim().ToLower())))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private String ExtractGenderFromQuery(String query)
        {
            GenderMappingSettings genderSettings = _config.IndexSettings.SearchSettings?.GenderMappingSettings ?? new GenderMappingSettings();

            String lowerQuery = query.ToLower();

            // Check English mappings
            Dictionary<String, String> englishMappings = genderSettings.EnglishMappings;
            foreach (KeyValuePair<String, String> mapping in englishMappings)
            {
                if (lowerQuery.Contains(mapping.Key.ToLower()))
                    return mapping.Value;
            }

            // Check Greek mappings
            Dictionary<String, String> greekMappings = genderSettings.GreekMappings;
            foreach (KeyValuePair<String, String> mapping in greekMappings)
            {
                if (lowerQuery.Contains(mapping.Key.ToLower()))
                    return mapping.Value;
            }

            return null;
        }

        private String GetNumberFromMatch(Match match)
        {
            // Try to get the numeric group from various possible positions in the regex match
            for (int i = 1; i < match.Groups.Count; i++)
            {
                String group = match.Groups[i].Value;
                if (!String.IsNullOrEmpty(group) && Regex.IsMatch(group, @"^\d+(?:\.\d+)?$"))
                {
                    return group;
                }
            }
            return String.Empty;
        }

        private Boolean IsMonthPattern(String matchValue, List<String> monthKeywords)
        {
            String lowerMatch = matchValue.ToLower();
            return monthKeywords.Any(keyword => lowerMatch.Contains(keyword.ToLower()));
        }

        private Boolean IsPoundPattern(String matchValue, List<String> poundKeywords)
        {
            String lowerMatch = matchValue.ToLower();
            return poundKeywords.Any(keyword => lowerMatch.Contains(keyword.ToLower()));
        }

        private Int32 GetDynamicMinimumShouldMatch(String query, QueryAnalysis queryAnalysis)
        {
            Int32 wordCount = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            if (queryAnalysis.MatchType == SearchMatchType.Exact)
            {
                return wordCount; 
            }
            else if (queryAnalysis.MatchType == SearchMatchType.Phrase)
            {
                return Math.Max(2, wordCount - 1); 
            }
            else if (wordCount >= 5)
            {
                return Math.Max(3, (Int32)(wordCount * 0.8)); 
            }
            else if (wordCount >= 3)
            {
                return Math.Max(2, wordCount - 1);
            }
            else
            {
                return 1;
            }
        }

        private Double GetDynamicSemanticThreshold(String query, QueryAnalysis queryAnalysis)
        {
            Double baseThreshold = _config.IndexSettings.TextScoreThreshold;
            ThresholdSettings thresholdSettings = _config.IndexSettings.SearchSettings?.ThresholdSettings ?? new ThresholdSettings();

            if (!thresholdSettings.DynamicThresholds)
            {
                return baseThreshold;
            }

            Int32 wordCount = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

            Double threshold = queryAnalysis.MatchType switch
            {
                SearchMatchType.Exact => baseThreshold * 5.0,
                SearchMatchType.Phrase => baseThreshold * 4.0,
                SearchMatchType.Mixed => baseThreshold * 3.0,
                SearchMatchType.Fuzzy => baseThreshold * 2.0,
                SearchMatchType.Descriptive => baseThreshold * 1.5,
                _ => baseThreshold * thresholdSettings.BaseMultiplier
            };

            if (thresholdSettings.WordCountAdjustment)
            {
                if (wordCount == 1)
                {
                    threshold *= 1.5;
                }
                else if (wordCount > 5)
                {
                    threshold *= 0.8;
                }
            }

            if (thresholdSettings.ContextualAdjustment && (queryAnalysis.HasGenderTerms || queryAnalysis.HasNumericTerms))
            {
                threshold *= 0.9;
            }

            return threshold;
        }

        private Double NormalizeSemanticScore(Double rawScore, String query, QueryAnalysis queryAnalysis)
        {
            Double maxExpectedScore = queryAnalysis.MatchType switch
            {
                SearchMatchType.Exact => 150.0,
                SearchMatchType.Phrase => 120.0,
                SearchMatchType.Mixed => 100.0,
                SearchMatchType.Fuzzy => 80.0,
                SearchMatchType.Descriptive => 60.0,
                _ => 100.0
            };

            Double normalizedScore = rawScore / maxExpectedScore;
            Double steepness = queryAnalysis.MatchType == SearchMatchType.Exact ? 8.0 : 5.0;

            return 1.0 / (1.0 + Math.Exp(-steepness * (normalizedScore - 0.5)));
        }

        private Double GetAgeTolerance(Double age)
        {
            if (_config.IndexSettings.MultilingualSettings?.NumericSearchMappings == null)
                return age <= 1 ? 0.25 : age <= 5 ? 0.5 : 1.0;

            NumericSearchMapping ageMapping = _config.IndexSettings.MultilingualSettings.NumericSearchMappings
                .FirstOrDefault(m => m.FieldName == "age");

            if (ageMapping == null)
                return 0.5;

            NumericRange range = ageMapping.RangeMappings
                .FirstOrDefault(r => age >= r.MinValue && age <= r.MaxValue);

            return range?.RangeTolerance ?? 0.5;
        }

        private Double GetWeightTolerance(Double weight)
        {
            if (_config.IndexSettings.MultilingualSettings?.NumericSearchMappings == null)
                return weight <= 10 ? 2.0 : weight <= 50 ? 5.0 : 10.0;

            NumericSearchMapping weightMapping = _config.IndexSettings.MultilingualSettings.NumericSearchMappings
                .FirstOrDefault(m => m.FieldName == "weight");

            if (weightMapping == null)
                return 5.0;

            NumericRange range = weightMapping.RangeMappings
                .FirstOrDefault(r => weight >= r.MinValue && weight <= r.MaxValue);

            return range?.RangeTolerance ?? 5.0;
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

            List<Data.Entities.Animal> finalResults = new List<Data.Entities.Animal>();
            HashSet<String> processedIds = new HashSet<String>();

            // Find animals that appear in both results (dual results)
            List<AnimalSearchResult> dualResults = new List<AnimalSearchResult>();
            Dictionary<String, AnimalSearchResult> semanticMap = semanticResults.ToDictionary(x => x.Animal.Id, x => x);

            foreach (AnimalSearchResult vectorResult in vectorResults)
            {
                String id = vectorResult.Animal.Id;
                if (semanticMap.TryGetValue(id, out AnimalSearchResult semanticResult))
                {
                    Double combinedScore = (vectorResult.VectorScore + semanticResult.SemanticScore) / 2.0;
                    dualResults.Add(new AnimalSearchResult
                    {
                        Animal = vectorResult.Animal,
                        VectorScore = vectorResult.VectorScore,
                        SemanticScore = semanticResult.SemanticScore,
                        CombinedScore = combinedScore,
                        VectorRank = vectorResult.VectorRank,
                        SemanticRank = semanticResult.SemanticRank
                    });
                    processedIds.Add(id);
                }
            }

            // Add dual results first (these count toward both vector and semantic quotas)
            List<AnimalSearchResult> sortedDualResults = dualResults
                .OrderByDescending(x => x.CombinedScore)
                .ToList();

            foreach (AnimalSearchResult result in sortedDualResults)
            {
                if (finalResults.Count >= this.PageSize) break;
                finalResults.Add(result.Animal);
            }

            // Calculate 50-50 distribution for remaining slots
            Int32 remainingSlots = this.PageSize - finalResults.Count;
            Int32 vectorQuota = remainingSlots / 2;
            Int32 semanticQuota = remainingSlots - vectorQuota; 

            // Add remaining vector-only results (50% of remaining slots)
            List<AnimalSearchResult> remainingVectorResults = vectorResults
                .Where(x => !processedIds.Contains(x.Animal.Id))
                .OrderByDescending(x => x.VectorScore)
                .ToList();

            Int32 vectorAdded = 0;
            foreach (AnimalSearchResult result in remainingVectorResults)
            {
                if (vectorAdded >= vectorQuota || finalResults.Count >= this.PageSize) break;
                finalResults.Add(result.Animal);
                processedIds.Add(result.Animal.Id);
                vectorAdded++;
            }

            // Add remaining semantic-only results (50% of remaining slots)
            List<AnimalSearchResult> remainingSemanticResults = semanticResults
                .Where(x => !processedIds.Contains(x.Animal.Id))
                .OrderByDescending(x => x.SemanticScore)
                .ToList();

            Int32 semanticAdded = 0;
            foreach (AnimalSearchResult result in remainingSemanticResults)
            {
                if (semanticAdded >= semanticQuota || finalResults.Count >= this.PageSize) break;
                finalResults.Add(result.Animal);
                processedIds.Add(result.Animal.Id);
                semanticAdded++;
            }

            // Fill any remaining slots if one type has more results available
            // First try to fill with remaining vector results
            foreach (AnimalSearchResult result in remainingVectorResults.Skip(vectorAdded))
            {
                if (finalResults.Count >= this.PageSize) break;
                if (!processedIds.Contains(result.Animal.Id))
                {
                    finalResults.Add(result.Animal);
                    processedIds.Add(result.Animal.Id);
                }
            }

            // Then try to fill with remaining semantic results
            foreach (AnimalSearchResult result in remainingSemanticResults.Skip(semanticAdded))
            {
                if (finalResults.Count >= this.PageSize) break;
                if (!processedIds.Contains(result.Animal.Id))
                {
                    finalResults.Add(result.Animal);
                    processedIds.Add(result.Animal.Id);
                }
            }

            return finalResults;
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
    }
}