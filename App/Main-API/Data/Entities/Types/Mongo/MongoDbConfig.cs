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
        public int Dims { get; set; }
        public int NumCandidates { get; set; }
        public int Topk { get; set; }
        public Double VectorScoreThreshold { get; set; }
        public Double TextScoreThreshold { get; set; }
        public List<IndexSynonyms> SynonymsBatch { get; set; }
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
        public List<LanguageMapping> LanguageMappings { get; set; }
        public List<MultilingualSynonym> MultilingualSynonyms { get; set; }
        public List<AgeKeywordMapping> AgeKeywords { get; set; }
        public List<GenderMapping> GenderMappings { get; set; }
        public List<HealthStatusMapping> HealthStatusMappings { get; set; }
        public List<NumericSearchMapping> NumericSearchMappings { get; set; }
        public String DefaultLanguage { get; set; }
        public String DefaultAnalyzer { get; set; }
    }

    public class LanguageMapping
    {
        public String Language { get; set; }
        public String Analyzer { get; set; }
        public List<String> UnicodeRanges { get; set; }
        public List<String> SearchPaths { get; set; }
    }

    public class MultilingualSynonym
    {
        public String Category { get; set; }
        public String Language { get; set; }
        public List<String> Terms { get; set; }
        public List<String> EnglishEquivalents { get; set; }
    }

    public class AgeKeywordMapping
    {
        public String Language { get; set; }
        public String Keyword { get; set; }
        public Double MinAge { get; set; }
        public Double MaxAge { get; set; }
    }

    public class GenderMapping
    {
        public String Language { get; set; }
        public String Term { get; set; }
        public String Gender { get; set; }
    }

    public class HealthStatusMapping
    {
        public String Language { get; set; }
        public String Term { get; set; }
        public List<String> EnglishEquivalents { get; set; }
    }

    public class NumericSearchMapping
    {
        public String FieldName { get; set; }
        public List<NumericRange> RangeMappings { get; set; }
        public Double BoostValue { get; set; }
    }

    public class NumericRange
    {
        public Double MinValue { get; set; }
        public Double MaxValue { get; set; }
        public Double RangeTolerance { get; set; }
    }

    public class SearchSettings
    {
        public FuzzySettings FuzzySettings { get; set; }
        public BoostSettings BoostSettings { get; set; }
        public FieldMappings FieldMappings { get; set; }
        public SearchCombinationSettings CombinationSettings { get; set; }
    }

    public class FuzzySettings
    {
        public int DefaultMaxEdits { get; set; }
        public int DefaultPrefixLength { get; set; }
        public int DefaultMaxExpansions { get; set; }
        public Dictionary<String, FuzzyConfiguration> FieldSpecificSettings { get; set; }
    }

    public class FuzzyConfiguration
    {
        public int MaxEdits { get; set; }
        public int PrefixLength { get; set; }
        public int MaxExpansions { get; set; }
    }

    public class BoostSettings
    {
        public Dictionary<String, Double> CategoryBoosts { get; set; }
        public Dictionary<String, Double> FieldBoosts { get; set; }
        public Dictionary<String, Double> OperatorBoosts { get; set; }
    }

    public class FieldMappings
    {
        public List<String> DescriptionFields { get; set; }
        public List<String> NameFields { get; set; }
        public List<String> HealthStatusFields { get; set; }
        public Dictionary<String, List<String>> CategoryFieldMappings { get; set; }
    }

    public class SearchCombinationSettings
    {
        public Double VectorWeight { get; set; }
        public Double SemanticWeight { get; set; }
        public int MinimumShouldMatch { get; set; }
        public Boolean EnablePhraseBoosting { get; set; }
        public Double PhraseBoosting { get; set; }
    }
}
