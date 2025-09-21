using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Mongo;
using Pawfect_API.Data.Entities.Types.Search;

namespace Pawfect_API.Models.Animal
{
    public class AnimalSearchDataModel : ISearchDataModel<String>
    {
        private readonly MongoDbConfig _config;
        public AnimalSearchDataModel
        (
            MongoDbConfig config,
            Double age,
            Gender gender,
            String description,
            Double weight,
            String healthStatus,
            String breedName,
            String breedDescription,
            String animalTypeName,
            String animalTypeDescription
        )
        {
            this._config = config ?? throw new ArgumentNullException(nameof(config));
            this.Age = age.ToString();
            this.Gender = gender.ToString();
            this.Description = description ?? String.Empty;
            this.Weight = weight.ToString();
            this.HealthStatus = healthStatus ?? String.Empty;
            this.BreedName = breedName ?? String.Empty;
            this.BreedDescription = breedDescription ?? String.Empty;
            this.AnimalTypeName = animalTypeName ?? String.Empty;
            this.AnimalTypeDescription = animalTypeDescription ?? String.Empty;
        }

        public String Age { get; set; }
        public String Gender { get; set; }
        public String Description { get; set; }
        public String Weight { get; set; }
        public String HealthStatus { get; set; }

        public String BreedName { get; set; }

        public String BreedDescription { get; set; }

        public String AnimalTypeName { get; set; }

        public String AnimalTypeDescription { get; set; }

        public String ToSearchText()
        {
            List<String> embeddingParts = new List<String>();

            // Animal Type - Most Important (with all synonyms)
            if (!String.IsNullOrWhiteSpace(AnimalTypeName))
            {
                // Add with emphasis
                embeddingParts.Add($"This is a {AnimalTypeName}");
                embeddingParts.Add($"Animal type: {AnimalTypeName}");

                // Add all matching synonyms from _config
                HashSet<String> synonyms = FindSynonyms(AnimalTypeName, "animal_types", _config);
                if (synonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", synonyms));
                }
            }

            // Breed Information with synonyms
            if (!String.IsNullOrWhiteSpace(BreedName))
            {
                embeddingParts.Add($"Breed: {BreedName}");

                // Check both dog and cat breed categories
                HashSet<String> dogBreedSynonyms = FindSynonyms(BreedName, "dog_breeds", _config);
                HashSet<String> catBreedSynonyms = FindSynonyms(BreedName, "cat_breeds", _config);
                IEnumerable<String> allBreedSynonyms = dogBreedSynonyms.Concat(catBreedSynonyms).Distinct();

                if (allBreedSynonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", allBreedSynonyms));
                }
            }

            //  Gender with synonyms
            if (!String.IsNullOrWhiteSpace(Gender))
            {
                embeddingParts.Add($"Gender: {Gender}");

                HashSet<String> genderSynonyms = FindSynonyms(Gender, "gender_related", _config);
                if (genderSynonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", genderSynonyms));
                }
            }

            // Age with mapped keywords
            if (!String.IsNullOrWhiteSpace(Age) && Double.TryParse(Age, out Double ageValue))
            {
                embeddingParts.Add($"Age: {Age} years old");

                // Add all matching age keywords from _config
                HashSet<String> ageKeywords = GetAgeKeywords(ageValue, _config);
                if (ageKeywords.Any())
                {
                    embeddingParts.Add(String.Join(" ", ageKeywords));
                }

                // Add age category synonyms
                HashSet<String> ageSynonyms = GetAgeCategorySynonyms(ageValue, _config);
                if (ageSynonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", ageSynonyms));
                }
            }

            // Weight with size synonyms
            if (!String.IsNullOrWhiteSpace(Weight) && Double.TryParse(Weight, out Double weightValue))
            {
                embeddingParts.Add($"Weight: {Weight} kg");

                // Find matching size synonyms based on weight ranges
                HashSet<String> sizeSynonyms = GetSizeSynonyms(weightValue, _config);
                if (sizeSynonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", sizeSynonyms));
                }
            }

            //  Health Status with synonyms
            if (!String.IsNullOrWhiteSpace(HealthStatus))
            {
                embeddingParts.Add($"Health: {HealthStatus}");

                // Find health status synonyms
                HashSet<String> healthSynonyms = FindHealthSynonyms(HealthStatus, _config);
                if (healthSynonyms.Any())
                {
                    embeddingParts.Add(String.Join(" ", healthSynonyms));
                }
            }

            // Description with extracted keywords
            if (!String.IsNullOrWhiteSpace(Description))
            {
                // Limit description to prevent overwhelming
                embeddingParts.Add($"Description: {Description}");

                // Extract keywords from description using _config categories
                HashSet<String> extractedKeywords = ExtractKeywordsFromText(Description, _config);
                if (extractedKeywords.Any())
                {
                    embeddingParts.Add(String.Join(" ", extractedKeywords.Take(15))); // Limit keywords
                }
            }

            // Breed Description (truncated)
            if (!String.IsNullOrWhiteSpace(BreedDescription))
            {
                embeddingParts.Add($"About breed: {BreedDescription}");
            }

            // 9. Animal Type Description (truncated)
            if (!String.IsNullOrWhiteSpace(AnimalTypeDescription))
            {
                embeddingParts.Add($"Type info: {AnimalTypeDescription}");
            }

            // Join all parts and clean up
            return String.Join(". ", embeddingParts.Where(p => !String.IsNullOrWhiteSpace(p)))
                .ToLower()
                .Trim();
        }

        // Find synonyms for a term in a specific category
        private HashSet<String> FindSynonyms(String term, String category, MongoDbConfig _config)
        {
            HashSet<String> synonyms = new HashSet<String>();
            String lowerTerm = term?.ToLower() ?? "";

            IndexSynonyms categoryGroup = _config.IndexSettings?.SynonymsBatch?
                .FirstOrDefault(s => s.Category == category);

            if (categoryGroup != null)
            {
                foreach (String synonymGroup in categoryGroup.Synonyms)
                {
                    String[] terms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    // If any term in the group matches, add all terms
                    if (terms.Any(t => lowerTerm.Contains(t) || t.Contains(lowerTerm)))
                    {
                        foreach (String synonym in terms)
                        {
                            synonyms.Add(synonym);
                        }
                        break; // Found matching group
                    }
                }
            }

            return synonyms;
        }

        // Get age keywords based on age value
        private HashSet<String> GetAgeKeywords(Double ageValue, MongoDbConfig _config)
        {
            HashSet<String> keywords = new HashSet<String>();

            List<AgeKeywordMapping> ageKeywordMappings = _config.IndexSettings?.MultilingualSettings?.AgeKeywords;
            if (ageKeywordMappings != null)
            {
                // Find all matching age ranges
                List<AgeKeywordMapping> matchingMappings = ageKeywordMappings
                    .Where(m => ageValue >= m.MinAge && ageValue <= m.MaxAge)
                    .ToList();

                foreach (AgeKeywordMapping mapping in matchingMappings)
                {
                    keywords.Add(mapping.Keyword.ToLower());
                }
            }

            return keywords;
        }

        // Get age category synonyms
        private HashSet<String> GetAgeCategorySynonyms(Double ageValue, MongoDbConfig _config)
        {
            HashSet<String> synonyms = new HashSet<String>();

            // Get age keywords first to determine categories
            HashSet<String> ageKeywords = GetAgeKeywords(ageValue, _config);

            // Find matching synonyms from ages category
            IndexSynonyms ageSynonyms = _config.IndexSettings?.SynonymsBatch?
                .FirstOrDefault(s => s.Category == "ages");

            if (ageSynonyms != null)
            {
                foreach (String synonymGroup in ageSynonyms.Synonyms)
                {
                    String[] terms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    // If any of our age keywords match a term in this group, add all terms
                    if (terms.Any(t => ageKeywords.Contains(t)))
                    {
                        foreach (String term in terms)
                        {
                            synonyms.Add(term);
                        }
                    }
                }
            }

            return synonyms;
        }

        // Get size synonyms based on weight
        private HashSet<String> GetSizeSynonyms(Double weightValue, MongoDbConfig _config)
        {
            HashSet<String> synonyms = new HashSet<String>();

            // Use weight ranges from _config to determine size category
            NumericSearchMapping weightMapping = _config.IndexSettings?.MultilingualSettings?.NumericSearchMappings?
                .FirstOrDefault(m => m.FieldName == "weight");

            // Map weight to size categories using ranges
            String sizeCategory = "";
            if (weightMapping?.RangeMappings != null)
            {
                // Determine size based on weight ranges
                if (weightValue < 5) sizeCategory = "tiny";
                else if (weightValue < 10) sizeCategory = "small";
                else if (weightValue < 25) sizeCategory = "medium";
                else if (weightValue < 40) sizeCategory = "large";
                else sizeCategory = "giant";
            }

            // Find matching size synonyms
            IndexSynonyms sizeSynonyms = _config.IndexSettings?.SynonymsBatch?
                .FirstOrDefault(s => s.Category == "sizes");

            if (sizeSynonyms != null && !String.IsNullOrEmpty(sizeCategory))
            {
                foreach (String synonymGroup in sizeSynonyms.Synonyms)
                {
                    String[] terms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    if (terms.Any(t => t.Contains(sizeCategory)))
                    {
                        foreach (String term in terms)
                        {
                            synonyms.Add(term);
                        }
                        break;
                    }
                }
            }

            return synonyms;
        }

        // Find health status synonyms
        private HashSet<String> FindHealthSynonyms(String healthStatus, MongoDbConfig _config)
        {
            HashSet<String> synonyms = new HashSet<String>();
            String lowerStatus = healthStatus?.ToLower() ?? "";

            IndexSynonyms healthSynonyms = _config.IndexSettings?.SynonymsBatch?
                .FirstOrDefault(s => s.Category == "health_status");

            if (healthSynonyms != null)
            {
                foreach (String synonymGroup in healthSynonyms.Synonyms)
                {
                    String[] terms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                    // Check if health status contains any of the terms
                    if (terms.Any(t => lowerStatus.Contains(t)))
                    {
                        foreach (String term in terms)
                        {
                            synonyms.Add(term);
                        }
                    }
                }
            }

            return synonyms;
        }

        // Extract keywords from text using all _config categories
        private HashSet<String> ExtractKeywordsFromText(String text, MongoDbConfig _config)
        {
            HashSet<String> keywords = new HashSet<String>();
            String lowerText = text?.ToLower() ?? "";

            if (String.IsNullOrWhiteSpace(lowerText))
                return keywords;

            // Check all synonym categories for matches
            List<IndexSynonyms> allCategories = _config.IndexSettings?.SynonymsBatch;
            if (allCategories != null)
            {
                // Priority categories for description extraction
                String[] priorityCategories = new[] {
                    "personalities", "training_behavior", "colors_patterns",
                    "coat_types", "special_needs", "living_situations"
                };

                foreach (IndexSynonyms category in allCategories)
                {
                    // Only process relevant categories for descriptions
                    if (!priorityCategories.Contains(category.Category))
                        continue;

                    foreach (String synonymGroup in category.Synonyms)
                    {
                        String[] terms = synonymGroup.Split(',').Select(s => s.Trim().ToLower()).ToArray();

                        // Check if any term appears in the text
                        foreach (String term in terms)
                        {
                            if (lowerText.Contains(term))
                            {
                                // Add the matched term (not all synonyms to avoid explosion)
                                keywords.Add(term);

                                // Optionally add a few related terms
                                foreach (String relatedTerm in terms.Take(3))
                                {
                                    keywords.Add(relatedTerm);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return keywords;
        }
    }
}
