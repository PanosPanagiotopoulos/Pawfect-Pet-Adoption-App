namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Mongo
{
    public class MongoDbConfig
    {
        public String ConnectionString { get; set; }
        public String DatabaseName { get; set; }
        public IndexSettings IndexSettings { get; set; }
    }

    public class IndexSettings
    {
        public PlainTextIndexNames PlainTextIndexNames { get; set; }
        public String AnimalVectorSearchIndexName { get; set; }
        public String AnimalSchemanticIndexName { get; set; }
        public Int32 Dims { get; set; }
        public Int32 NumCandidates { get; set; }
        public Int32 Topk { get; set; }
        public Double VectorScoreThreshold { get; set; }
        public Double TextScoreThreshold { get; set; }
        public List<IndexSynonyms> SynonymsBatch { get; set; }
        public List<IndexSynonyms> GreekSynonymsBatch { get; set; }
        public MultilingualSettings MultilingualSettings { get; set; }
        public SearchSettings SearchSettings { get; set; }
    }

    public class PlainTextIndexNames
    {
        public String UserFullNameTextIndex { get; set; }
        public String UserFullNameRegexIndex { get; set; }
        public String ShelterNameTextIndex { get; set; }
        public String ShelterNameRegexIndex { get; set; }
        public String FileNameTextIndex { get; set; }
        public String FileNameRegexIndex { get; set; }
        public String AnimalTypeNameTextIndex { get; set; }
        public String AnimalTypeNameRegexIndex { get; set; }
        public String BreedNameTextIndex { get; set; }
        public String BreedNameRegexIndex { get; set; }
    }

    public class IndexSynonyms
    {
        public String Category { get; set; }
        public List<String> Synonyms { get; set; }
    }

    public class MultilingualSettings
    {
        public List<AgeKeywordMapping> AgeKeywords { get; set; }
        public List<NumericSearchMapping> NumericSearchMappings { get; set; }
        public String DefaultLanguage { get; set; }
        public String DefaultAnalyzer { get; set; }
    }

    public class AgeKeywordMapping
    {
        public String Language { get; set; }
        public String Keyword { get; set; }
        public Double MinAge { get; set; }
        public Double MaxAge { get; set; }
    }

    public class NumericSearchMapping
    {
        public String FieldName { get; set; }
        public List<NumericRange> RangeMappings { get; set; }
    }

    public class NumericRange
    {
        public Double MinValue { get; set; }
        public Double MaxValue { get; set; }
        public Double RangeTolerance { get; set; }
    }

    public class SearchSettings
    {
        public SearchCombinationSettings CombinationSettings { get; set; }
        public BoostSettings BoostSettings { get; set; }
        public ThresholdSettings ThresholdSettings { get; set; }
        public FuzzySettings FuzzySettings { get; set; }
        public NumericPatternSettings NumericPatternSettings { get; set; }
        public GenderMappingSettings GenderMappingSettings { get; set; }
        public KeywordSettings KeywordSettings { get; set; }
    }

    public class SearchCombinationSettings
    {
        public Double VectorWeight { get; set; }
        public Double SemanticWeight { get; set; }
    }

    public class BoostSettings
    {
        public Double ExactPhraseBoost { get; set; }
        public Double PhraseWithSlopBoost { get; set; }
        public Double SemanticTextBoost { get; set; }
        public Double SynonymBoost { get; set; }
        public Double DescriptionBoost { get; set; }
        public Double FuzzySearchBoost { get; set; }
        public Double HealthTermsBoost { get; set; }
        public Double AutocompleteBoost { get; set; }
        public Double GenderExactBoost { get; set; }
        public Double GenderPhraseBoost { get; set; }
        public Double NumericRangeBoost { get; set; }
        public Double WeightRangeBoost { get; set; }
    }

    public class ThresholdSettings
    {
        public Boolean DynamicThresholds { get; set; }
        public Double BaseMultiplier { get; set; }
        public Boolean WordCountAdjustment { get; set; }
        public Boolean ContextualAdjustment { get; set; }
    }

    public class FuzzySettings
    {
        public Int32 MaxEdits { get; set; }
        public Int32 MinQueryLength { get; set; }
        public Int32 MaxExpansions { get; set; }
    }

    public class NumericPatternSettings
    {
        public List<String> AgePatterns { get; set; }
        public List<String> WeightPatterns { get; set; }
        public List<String> MonthKeywords { get; set; }
        public List<String> PoundKeywords { get; set; }
        public Double PoundToKgConversion { get; set; }
        public Double MonthToYearConversion { get; set; }
    }

    public class GenderMappingSettings
    {
        public Dictionary<String, String> EnglishMappings { get; set; }
        public Dictionary<String, String> GreekMappings { get; set; }
    }

    public class KeywordSettings
    {
        public List<String> AgeKeywords { get; set; }
        public List<String> WeightKeywords { get; set; }
        public List<String> PersonalityKeywords { get; set; }
    }
}
