namespace Pawfect_Messenger.Data.Entities.Types.Mongo
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
        public int Dims { get; set; }
        public int NumCandidates { get; set; }
        public int Topk { get; set; }
        public double VectorScoreThreshold { get; set; }
        public double TextScoreThreshold { get; set; }
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
        public double MinAge { get; set; }
        public double MaxAge { get; set; }
    }

    public class NumericSearchMapping
    {
        public String FieldName { get; set; }
        public List<NumericRange> RangeMappings { get; set; }
    }

    public class NumericRange
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double RangeTolerance { get; set; }
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
        public double VectorWeight { get; set; }
        public double SemanticWeight { get; set; }
    }

    public class BoostSettings
    {
        public double ExactPhraseBoost { get; set; }
        public double PhraseWithSlopBoost { get; set; }
        public double SemanticTextBoost { get; set; }
        public double SynonymBoost { get; set; }
        public double DescriptionBoost { get; set; }
        public double FuzzySearchBoost { get; set; }
        public double HealthTermsBoost { get; set; }
        public double AutocompleteBoost { get; set; }
        public double GenderExactBoost { get; set; }
        public double GenderPhraseBoost { get; set; }
        public double NumericRangeBoost { get; set; }
        public double WeightRangeBoost { get; set; }
    }

    public class ThresholdSettings
    {
        public Boolean DynamicThresholds { get; set; }
        public double BaseMultiplier { get; set; }
        public Boolean WordCountAdjustment { get; set; }
        public Boolean ContextualAdjustment { get; set; }
    }

    public class FuzzySettings
    {
        public int MaxEdits { get; set; }
        public int MinQueryLength { get; set; }
        public int MaxExpansions { get; set; }
    }

    public class NumericPatternSettings
    {
        public List<String> AgePatterns { get; set; }
        public List<String> WeightPatterns { get; set; }
        public List<String> MonthKeywords { get; set; }
        public List<String> PoundKeywords { get; set; }
        public double PoundToKgConversion { get; set; }
        public double MonthToYearConversion { get; set; }
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
