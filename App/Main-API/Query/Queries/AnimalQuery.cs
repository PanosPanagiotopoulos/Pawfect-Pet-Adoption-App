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
using MongoDB.Bson.Serialization;
using Pawfect_Pet_Adoption_App_API.Services.TranslationServices;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation;

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
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver authorizationContentResolver,
            IEmbeddingService _embeddingService,
            ITranslationService translationService,
            IOptions<MongoDbConfig> options

        ) : base(mongoDbService, AuthorizationService, authorizationContentResolver, claimsExtractor)
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

            String query = this.CleanQuery();
            
            Task<List<Data.Entities.Animal>> vectorSearchTask = this.AnimalVectorSearch(filterDoc, query);
            Task<List<Data.Entities.Animal>> semanticSearchTask = this.AnimalSemanticSearch(filterDoc, query);

            List<Data.Entities.Animal>[] results = await Task.WhenAll(vectorSearchTask, semanticSearchTask);

            List<Data.Entities.Animal> vectorResults = results[0];
            List<Data.Entities.Animal> semanticResults = results[1];

            List<Data.Entities.Animal> combinedResults = this.CombineSearchResults(vectorResults, semanticResults);

            return combinedResults;
        }

        #region Vector Search
        private async Task<List<Data.Entities.Animal>> AnimalVectorSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<Data.Entities.Animal>();

            String translatedQuery = await _translationService.TranslateAsync(query, null, SupportedLanguages.English);

            Double[] queryEmbedding = (await _embeddingService.GenerateEmbeddingAsyncDouble(translatedQuery)).Vector.ToArray();

            int totalNeeded = Math.Max(this.Offset - 1, 0) * this.PageSize + this.PageSize;
            int vectorSearchLimit = Math.Max(totalNeeded * 2, _config.IndexSettings.Topk);

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
                    { "limit", vectorSearchLimit }
                }),
        
                // Add vector score
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "searchScore", new BsonDocument("$meta", "vectorSearchScore") }
                }),
        
                // Apply score threshold
                new BsonDocument("$match", new BsonDocument
                {
                    { "searchScore", new BsonDocument("$gte", _config.IndexSettings.VectorScoreThreshold) }
                })
            };

            // Add base filters if any
            if (baseFilter != null && baseFilter.ElementCount > 0) pipeline.Add(new BsonDocument("$match", baseFilter));

            // Apply sorting, pagination, and projection
            pipeline = this.ApplySorting(pipeline);
            pipeline = this.ApplyPagination(pipeline);
            pipeline = this.ApplyProjection(pipeline);

            // Execute aggregation
            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;

            return await scopedCollection.InternalCollection.Aggregate<Data.Entities.Animal>(pipeline).ToListAsync();
        }

        #endregion

        #region Schemantic Search Filtering
        private async Task<List<Data.Entities.Animal>> AnimalSemanticSearch(BsonDocument baseFilter, String query)
        {
            if (String.IsNullOrWhiteSpace(query)) return new List<Data.Entities.Animal>();

            // Build semantic search queries
            List<BsonDocument> searchQueries = await this.BuildSemanticSearchQueries(query);

            if (!searchQueries.Any()) return new List<Data.Entities.Animal>();

            // Build semantic search aggregation pipeline
            List<BsonDocument> pipeline = new List<BsonDocument>
            {
                // Semantic search stage
                new BsonDocument("$search", new BsonDocument
                {
                    { "index", _config.IndexSettings.AnimalSchemanticIndexName },
                    { "compound", new BsonDocument
                        {
                            { "should", new BsonArray(searchQueries) },
                            { "minimumShouldMatch", this.GetMinimumShouldMatch() }
                        }
                    }
                }),

                // Add text score
                new BsonDocument("$addFields", new BsonDocument
                {
                    { "searchScore", new BsonDocument("$meta", "searchScore") }
                }),

                // Apply score threshold
                new BsonDocument("$match", new BsonDocument
                {
                    { "searchScore", new BsonDocument("$gte", _config.IndexSettings.TextScoreThreshold) }
                })
            };

            // Add base filters if any
            if (baseFilter != null && baseFilter.ElementCount > 0)
            {
                pipeline.Add(new BsonDocument("$match", baseFilter));
            }

            // Apply sorting, pagination, and projection
            pipeline = this.ApplySorting(pipeline);
            pipeline = this.ApplyPagination(pipeline);
            pipeline = this.ApplyProjection(pipeline);

            // Execute aggregation
            SessionScopedMongoCollection<Data.Entities.Animal> scopedCollection = (SessionScopedMongoCollection<Data.Entities.Animal>)_collection;

            return await scopedCollection.InternalCollection.Aggregate<Data.Entities.Animal>(pipeline).ToListAsync();
        }

        private async Task<List<BsonDocument>> BuildSemanticSearchQueries(String query)
        {
            List<BsonDocument> searchQueries = new List<BsonDocument>();

            // Detect language and get appropriate paths
            String detectedLanguage = this.DetectLanguage(query);
            String[] languagePaths = this.GetLanguageSpecificPaths(detectedLanguage);

            // Primary semantic search on description with configurable fields and boosts
            this.AddDescriptionSearch(searchQueries, query, languagePaths);

            // Multilingual name search with configurable settings
            this.AddNameSearch(searchQueries, query, languagePaths);

            // Configurable autocomplete searches
            this.AddAutocompleteSearches(searchQueries, query);

            // Enhanced multilingual health status search
            this.AddMultilingualHealthStatusSearch(searchQueries, query, detectedLanguage, languagePaths);

            // Enhanced configurable methods
            this.AddNumericSearchQueries(searchQueries, query);
            this.AddMultilingualGenderSearchQueries(searchQueries, query, detectedLanguage);
            this.AddMultilingualSynonymSearches(searchQueries, query, detectedLanguage);
            this.AddConfigBasedSynonymSearches(searchQueries, query);

            return await Task.FromResult(searchQueries);
        }

        private void AddDescriptionSearch(List<BsonDocument> searchQueries, String query, String[] languagePaths)
        {
            FieldMappings fieldMappings = this.GetFieldMappings();
            FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("description");
            Double boostValue = this.GetFieldBoostValue("description");

            List<String> descriptionFields = fieldMappings.DescriptionFields ?? new List<String> { "description" };
            IEnumerable<String> pathsWithLanguage = languagePaths.SelectMany(p => descriptionFields.Select(f => $"{f}.{p}")).Concat(descriptionFields);

            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray(pathsWithLanguage) },
                { "fuzzy", new BsonDocument
                    {
                        { "maxEdits", fuzzySettings.MaxEdits },
                        { "prefixLength", fuzzySettings.PrefixLength },
                        { "maxExpansions", fuzzySettings.MaxExpansions }
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
            }));
        }

        private void AddNameSearch(List<BsonDocument> searchQueries, String query, String[] languagePaths)
        {
            FieldMappings fieldMappings = this.GetFieldMappings();
            FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("name");
            Double boostValue = this.GetFieldBoostValue("name");

            List<String> nameFields = fieldMappings.NameFields ?? new List<String> { "name" };
            IEnumerable<String> pathsWithLanguage = languagePaths.SelectMany(p => nameFields.Select(f => $"{f}.{p}")).Concat(nameFields);

            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray(pathsWithLanguage) },
                { "fuzzy", new BsonDocument
                    {
                        { "maxEdits", fuzzySettings.MaxEdits },
                        { "maxExpansions", fuzzySettings.MaxExpansions }
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
            }));
        }

        private void AddAutocompleteSearches(List<BsonDocument> searchQueries, String query)
        {
            Double nameAutocompleteBoost = this.GetOperatorBoostValue("nameAutocomplete");
            Double descriptionAutocompleteBoost = this.GetOperatorBoostValue("descriptionAutocomplete");

            searchQueries.Add(new BsonDocument("autocomplete", new BsonDocument
            {
                { "query", query },
                { "path", "nameAutocomplete" },
                { "score", new BsonDocument("boost", new BsonDocument("value", nameAutocompleteBoost)) }
            }));

            searchQueries.Add(new BsonDocument("autocomplete", new BsonDocument
            {
                { "query", query },
                { "path", "descriptionAutocomplete" },
                { "score", new BsonDocument("boost", new BsonDocument("value", descriptionAutocompleteBoost)) }
            }));
        }

        private void AddNumericSearchQueries(List<BsonDocument> searchQueries, String query)
        {
            // First check for age-related keywords with multilingual support
            this.AddAgeKeywordSearches(searchQueries, query);

            // Then handle numeric values with configurable mappings
            if (Double.TryParse(query, out Double numericValue))
            {
                List<NumericSearchMapping> numericMappings = this.GetNumericSearchMappings();
                foreach (NumericSearchMapping mapping in numericMappings)
                {
                    NumericRange rangeConfig = this.GetRangeForValue(mapping.RangeMappings, numericValue);
                    if (rangeConfig != null)
                    {
                        searchQueries.Add(new BsonDocument("range", new BsonDocument
                        {
                            { "path", mapping.FieldName },
                            { "gte", Math.Max(0, numericValue - rangeConfig.RangeTolerance) },
                            { "lte", numericValue + rangeConfig.RangeTolerance },
                            { "score", new BsonDocument("boost", new BsonDocument("value", mapping.BoostValue)) }
                        }));
                    }
                }
            }
        }

        private void AddMultilingualGenderSearchQueries(List<BsonDocument> searchQueries, String query, String detectedLanguage)
        {
            MultilingualSettings multilingualSettings = _config.IndexSettings.MultilingualSettings;
            if (multilingualSettings?.GenderMappings == null)
            {
                return;
            }

            String lowerQuery = query.ToLower();
            Double boostValue = this.GetFieldBoostValue("gender");

            foreach (GenderMapping genderMapping in multilingualSettings.GenderMappings)
            {
                if (lowerQuery.Contains(genderMapping.Term.ToLower()))
                {
                    if (Enum.TryParse<Gender>(genderMapping.Gender, out Gender gender))
                    {
                        // Use both numeric value and String representation for better matching
                        searchQueries.Add(new BsonDocument("text", new BsonDocument
                        {
                            { "query", gender.ToString() },
                            { "path", "gender" },
                            { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                        }));

                        searchQueries.Add(new BsonDocument("equals", new BsonDocument
                        {
                            { "path", "gender" },
                            { "value", (int)gender },
                            { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                        }));
                    }
                    break;
                }
            }
        }

        private void AddAgeKeywordSearches(List<BsonDocument> searchQueries, String query)
        {
            MultilingualSettings multilingualSettings = _config.IndexSettings.MultilingualSettings;
            if (multilingualSettings?.AgeKeywords == null)
            {
                return;
            }

            String lowerQuery = query.ToLower();
            String detectedLanguage = this.DetectLanguage(query);
            Double boostValue = this.GetCategoryBoostValue("ages");

            foreach (AgeKeywordMapping ageKeyword in multilingualSettings.AgeKeywords.Where(a => a.Language == detectedLanguage))
            {
                if (lowerQuery.Contains(ageKeyword.Keyword.ToLower()))
                {
                    searchQueries.Add(new BsonDocument("range", new BsonDocument
                    {
                        { "path", "age" },
                        { "gte", ageKeyword.MinAge },
                        { "lte", ageKeyword.MaxAge },
                        { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                    }));
                    break;
                }
            }
        }

        private void AddConfigBasedSynonymSearches(List<BsonDocument> searchQueries, String query)
        {
            HashSet<String> processedSynonyms = new HashSet<String>();

            foreach (IndexSynonyms categoryGroup in _config.IndexSettings.SynonymsBatch)
            {
                foreach (String synonymGroup in categoryGroup.Synonyms)
                {
                    String[] synonyms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    if (synonyms.Any(synonym => query.Contains(synonym) || this.IsPartialMatch(query, synonym)))
                    {
                        foreach (String synonym in synonyms)
                        {
                            if (processedSynonyms.Add(synonym))
                            {
                                Double boostValue = this.GetCategoryBoostValue(categoryGroup.Category);
                                String[] searchPaths = this.GetSearchPathsForCategory(categoryGroup.Category);
                                FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("synonym");

                                searchQueries.Add(new BsonDocument("text", new BsonDocument
                                {
                                    { "query", synonym },
                                    { "path", new BsonArray(searchPaths) },
                                    { "fuzzy", new BsonDocument { { "maxEdits", fuzzySettings.MaxEdits } } },
                                    { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                                }));

                                if (this.IsHighValueTerm(synonym, categoryGroup.Category) && this.IsPhraseBoosingEnabled())
                                {
                                    Double phraseBoosting = this.GetPhraseBoosting();
                                    searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                                    {
                                        { "query", synonym },
                                        { "path", searchPaths[0] },
                                        { "score", new BsonDocument("boost", new BsonDocument("value", boostValue * phraseBoosting)) }
                                    }));
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        #region Multilingual Helper Methods

        private String DetectLanguage(String query)
        {
            if (String.IsNullOrWhiteSpace(query))
                return this.GetDefaultLanguage();

            MultilingualSettings multilingualSettings = _config.IndexSettings.MultilingualSettings;
            if (multilingualSettings?.LanguageMappings == null)
                return this.GetDefaultLanguage();

            foreach (LanguageMapping languageMapping in multilingualSettings.LanguageMappings)
            {
                if (languageMapping.UnicodeRanges != null)
                {
                    foreach (String range in languageMapping.UnicodeRanges)
                    {
                        if (this.ContainsCharactersInRange(query, range))
                        {
                            return languageMapping.Language;
                        }
                    }
                }
            }

            return this.GetDefaultLanguage();
        }

        private Boolean ContainsCharactersInRange(String text, String range)
        {
            if (String.IsNullOrWhiteSpace(text) || String.IsNullOrWhiteSpace(range)) return false;

            String[] parts = range.Split('-');
            if (parts.Length != 2) return false;

            if (int.TryParse(parts[0].Replace("U+", ""), System.Globalization.NumberStyles.HexNumber, null, out int start) &&
                int.TryParse(parts[1].Replace("U+", ""), System.Globalization.NumberStyles.HexNumber, null, out int end))
            {
                return text.Any(c => c >= start && c <= end);
            }

            return false;
        }

        private String[] GetLanguageSpecificPaths(String language)
        {
            LanguageMapping languageMapping = _config.IndexSettings.MultilingualSettings?.LanguageMappings?.FirstOrDefault(l => l.Language == language);
            return languageMapping?.SearchPaths?.ToArray() ?? new[] { language, "standard", "exact" };
        }

        private void AddMultilingualHealthStatusSearch(List<BsonDocument> searchQueries, String query, String detectedLanguage, String[] languagePaths)
        {
            FieldMappings fieldMappings = this.GetFieldMappings();
            FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("healthStatus");
            Double boostValue = this.GetFieldBoostValue("healthStatus");

            List<String> healthStatusFields = fieldMappings.HealthStatusFields ?? new List<String> { "healthStatus" };
            IEnumerable<String> pathsWithLanguage = languagePaths.SelectMany(p => healthStatusFields.Select(f => $"{f}.{p}")).Concat(healthStatusFields);

            searchQueries.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", query },
                { "path", new BsonArray(pathsWithLanguage) },
                { "fuzzy", new BsonDocument
                    {
                        { "maxEdits", fuzzySettings.MaxEdits },
                        { "maxExpansions", fuzzySettings.MaxExpansions }
                    }
                },
                { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
            }));

            if (query.Contains(" ") && this.IsPhraseBoosingEnabled())
            {
                Double phraseBoosting = this.GetPhraseBoosting();
                IEnumerable<String> allFields = pathsWithLanguage.Concat(languagePaths.SelectMany(p => fieldMappings.DescriptionFields.Select(f => $"{f}.{p}")));

                searchQueries.Add(new BsonDocument("phrase", new BsonDocument
                {
                    { "query", query },
                    { "path", new BsonArray(allFields) },
                    { "slop", 2 },
                    { "score", new BsonDocument("boost", new BsonDocument("value", boostValue * phraseBoosting)) }
                }));
            }

            this.AddConfigBasedHealthStatusMappings(searchQueries, query, detectedLanguage, languagePaths);
        }

        private void AddConfigBasedHealthStatusMappings(List<BsonDocument> searchQueries, String query, String detectedLanguage, String[] languagePaths)
        {
            MultilingualSettings multilingualSettings = _config.IndexSettings.MultilingualSettings;
            if (multilingualSettings?.HealthStatusMappings == null) return;

            String lowerQuery = query.ToLower();
            FieldMappings fieldMappings = this.GetFieldMappings();
            Double boostValue = this.GetCategoryBoostValue("health_status");
            FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("healthMapping");

            foreach (HealthStatusMapping healthMapping in multilingualSettings.HealthStatusMappings.Where(h => h.Language == detectedLanguage))
            {
                if (lowerQuery.Contains(healthMapping.Term.ToLower()))
                {
                    foreach (String englishEquivalent in healthMapping.EnglishEquivalents)
                    {
                        IEnumerable<String> allHealthPaths = languagePaths.SelectMany(p => fieldMappings.HealthStatusFields.Select(f => $"{f}.{p}"))
                            .Concat(languagePaths.SelectMany(p => fieldMappings.DescriptionFields.Select(f => $"{f}.{p}")));

                        searchQueries.Add(new BsonDocument("text", new BsonDocument
                        {
                            { "query", englishEquivalent },
                            { "path", new BsonArray(allHealthPaths) },
                            { "fuzzy", new BsonDocument { { "maxEdits", fuzzySettings.MaxEdits } } },
                            { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                        }));
                    }
                    break;
                }
            }
        }

        private void AddMultilingualSynonymSearches(List<BsonDocument> searchQueries, String query, String detectedLanguage)
        {
            MultilingualSettings multilingualSettings = _config.IndexSettings.MultilingualSettings;
            if (multilingualSettings?.MultilingualSynonyms == null) return;

            String lowerQuery = query.ToLower();
            HashSet<String> processedSynonyms = new HashSet<String>();

            foreach (MultilingualSynonym synonymGroup in multilingualSettings.MultilingualSynonyms.Where(s => s.Language == detectedLanguage))
            {
                foreach (String term in synonymGroup.Terms)
                {
                    if (lowerQuery.Contains(term.ToLower()))
                    {
                        foreach (String englishEquivalent in synonymGroup.EnglishEquivalents)
                        {
                            if (processedSynonyms.Add(englishEquivalent))
                            {
                                Double boostValue = this.GetCategoryBoostValue(synonymGroup.Category);
                                String[] searchPaths = this.GetSearchPathsForCategory(synonymGroup.Category);
                                FuzzyConfiguration fuzzySettings = this.GetFuzzySettings("multilingualSynonym");

                                searchQueries.Add(new BsonDocument("text", new BsonDocument
                        {
                            { "query", englishEquivalent },
                            { "path", new BsonArray(searchPaths) },
                            { "fuzzy", new BsonDocument { { "maxEdits", fuzzySettings.MaxEdits } } },
                            { "score", new BsonDocument("boost", new BsonDocument("value", boostValue)) }
                        }));
                            }
                        }
                        break;
                    }
                }
            }
        }

        #endregion

        #region Configuration Helper Methods

        private String GetDefaultLanguage()
        {
            return _config.IndexSettings.MultilingualSettings?.DefaultLanguage ?? "english";
        }

        private String GetDefaultAnalyzer()
        {
            return _config.IndexSettings.MultilingualSettings?.DefaultAnalyzer ?? "lucene.standard";
        }

        private int GetMinimumShouldMatch()
        {
            return _config.IndexSettings.SearchSettings?.CombinationSettings?.MinimumShouldMatch ?? 1;
        }

        private FuzzyConfiguration GetFuzzySettings(String context)
        {
            FuzzySettings searchSettings = _config.IndexSettings.SearchSettings?.FuzzySettings;
            if (searchSettings?.FieldSpecificSettings?.ContainsKey(context) == true)
            {
                return searchSettings.FieldSpecificSettings[context];
            }

            return new FuzzyConfiguration
            {
                MaxEdits = searchSettings?.DefaultMaxEdits ?? 2,
                PrefixLength = searchSettings?.DefaultPrefixLength ?? 1,
                MaxExpansions = searchSettings?.DefaultMaxExpansions ?? 50
            };
        }

        private Double GetFieldBoostValue(String fieldName)
        {
            return _config.IndexSettings.SearchSettings?.BoostSettings?.FieldBoosts?.GetValueOrDefault(fieldName) ?? 2.0;
        }

        private Double GetCategoryBoostValue(String category)
        {
            return _config.IndexSettings.SearchSettings?.BoostSettings?.CategoryBoosts?.GetValueOrDefault(category) ?? 2.0;
        }

        private Double GetOperatorBoostValue(String operatorName)
        {
            return _config.IndexSettings.SearchSettings?.BoostSettings?.OperatorBoosts?.GetValueOrDefault(operatorName) ?? 2.0;
        }

        private FieldMappings GetFieldMappings()
        {
            return _config.IndexSettings.SearchSettings?.FieldMappings ?? new FieldMappings
            {
                DescriptionFields = new List<String> { "description" },
                NameFields = new List<String> { "name" },
                HealthStatusFields = new List<String> { "healthStatus" },
                CategoryFieldMappings = new Dictionary<String, List<String>>()
            };
        }

        private List<NumericSearchMapping> GetNumericSearchMappings()
        {
            return _config.IndexSettings.MultilingualSettings?.NumericSearchMappings ?? new List<NumericSearchMapping>
    {
        new NumericSearchMapping
        {
            FieldName = "age",
            BoostValue = 2.5,
            RangeMappings = new List<NumericRange>
            {
                new NumericRange { MinValue = 0, MaxValue = 1, RangeTolerance = 0.5 },
                new NumericRange { MinValue = 1, MaxValue = 5, RangeTolerance = 1.0 },
                new NumericRange { MinValue = 5, MaxValue = 10, RangeTolerance = 2.0 },
                new NumericRange { MinValue = 10, MaxValue = Double.MaxValue, RangeTolerance = 3.0 }
            }
        }
    };
        }

        private NumericRange GetRangeForValue(List<NumericRange> ranges, Double value)
        {
            return ranges?.FirstOrDefault(r => value >= r.MinValue && value <= r.MaxValue);
        }

        private Boolean IsPhraseBoosingEnabled()
        {
            return _config.IndexSettings.SearchSettings?.CombinationSettings?.EnablePhraseBoosting ?? true;
        }

        private Double GetPhraseBoosting()
        {
            return _config.IndexSettings.SearchSettings?.CombinationSettings?.PhraseBoosting ?? 1.2;
        }

        private String[] GetSearchPathsForCategory(String category)
        {
            FieldMappings fieldMappings = this.GetFieldMappings();
            if (fieldMappings.CategoryFieldMappings?.ContainsKey(category) == true)
            {
                return fieldMappings.CategoryFieldMappings[category].ToArray();
            }

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

        #region Combine Results
        private List<Data.Entities.Animal> CombineSearchResults(
            List<Data.Entities.Animal> vectorResults,
            List<Data.Entities.Animal> semanticResults)
        {
            if (!vectorResults.Any() && !semanticResults.Any())
            {
                return new List<Data.Entities.Animal>();
            }

            if (!vectorResults.Any())
            {
                return semanticResults;
            }

            if (!semanticResults.Any())
            {
                return vectorResults;
            }

            SearchCombinationSettings combinationSettings = _config.IndexSettings.SearchSettings?.CombinationSettings;
            Double vectorWeight = combinationSettings?.VectorWeight ?? 0.6;
            Double semanticWeight = combinationSettings?.SemanticWeight ?? 0.4;

            Dictionary<String, (Data.Entities.Animal animal, Double combinedScore)> combinedMap = new Dictionary<String, (Data.Entities.Animal, Double)>();

            // Add vector results with configurable weight
            for (int i = 0; i < vectorResults.Count; i++)
            {
                Data.Entities.Animal animal = vectorResults[i];
                String id = animal.Id.ToString();

                Double vectorScore = (vectorResults.Count - i) / (Double)vectorResults.Count;
                Double weightedScore = vectorScore * vectorWeight;

                combinedMap[id] = (animal, weightedScore);
            }

            // Add semantic results with configurable weight
            for (int i = 0; i < semanticResults.Count; i++)
            {
                Data.Entities.Animal animal = semanticResults[i];
                String id = animal.Id.ToString();

                Double semanticScore = (semanticResults.Count - i) / (Double)semanticResults.Count;
                Double weightedScore = semanticScore * semanticWeight;

                if (combinedMap.ContainsKey(id))
                {
                    (Data.Entities.Animal animal, Double combinedScore) existing = combinedMap[id];
                    combinedMap[id] = (existing.animal, existing.combinedScore + weightedScore);
                }
                else
                {
                    combinedMap[id] = (animal, weightedScore);
                }
            }

            List<Data.Entities.Animal> finalResults = combinedMap.Values
                .OrderByDescending(x => x.combinedScore)
                .Select(x => x.animal)
                .ToList();

            int startIndex = Math.Max(this.Offset - 1, 0) * this.PageSize;

            if (startIndex >= finalResults.Count) return new List<Data.Entities.Animal>();

            return finalResults
                .Skip(startIndex)
                .Take(this.PageSize)
                .ToList();
        }
        #endregion

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

                if (item.StartsWith(nameof(Models.Animal.Animal.AttachedPhotos))) projectionFields.Add(nameof(Data.Entities.Animal.PhotosIds));
                if (item.StartsWith(nameof(Models.Animal.Animal.Shelter))) projectionFields.Add(nameof(Data.Entities.Animal.ShelterId));
                if (item.StartsWith(nameof(Models.Animal.Animal.Breed))) projectionFields.Add(nameof(Data.Entities.Animal.BreedId));
                if (item.StartsWith(nameof(Models.Animal.Animal.AnimalType))) projectionFields.Add(nameof(Data.Entities.Animal.AnimalTypeId));
            }

            return projectionFields.ToList();
        }

        #region Helpers

        private Boolean ContainsAllWords(String query, String[] words)
        {
            return words.All(word => query.Contains(word.ToLower()));
        }

        private Boolean IsPartialMatch(String query, String synonym)
        {
            if (synonym.Contains(" "))
            {
                String[] words = synonym.Split(' ');
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
            FieldMappings fieldMappings = this.GetFieldMappings();
            if (fieldMappings.CategoryFieldMappings?.ContainsKey($"{category}_highvalue") == true)
            {
                return fieldMappings.CategoryFieldMappings[$"{category}_highvalue"].Contains(synonym);
            }

            return category.ToLower() switch
            {
                "dog_breeds" or "cat_breeds" => true,
                "animal_types" => true,
                "health_status" => synonym.Length > 4,
                "adoption_status" => true,
                _ => false
            };
        }

        #endregion

    }
}