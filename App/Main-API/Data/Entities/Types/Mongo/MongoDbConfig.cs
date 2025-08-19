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
    }

    public class SearchCombinationSettings
    {
        public Double VectorWeight { get; set; }
        public Double SemanticWeight { get; set; }
    }
}
