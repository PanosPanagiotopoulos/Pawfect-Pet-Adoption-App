using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Data.Entities.Types.Search
{
    public class QueryAnalysis
    {
        public SearchMatchType MatchType { get; set; }
        public Boolean HasExactPhraseIntent { get; set; }
        public Boolean NeedsSynonymExpansion { get; set; }
        public Boolean AllowFuzzySearch { get; set; }
        public Boolean HasHealthTerms { get; set; }
        public Boolean HasGenderTerms { get; set; }
        public Boolean HasNumericTerms { get; set; }
        public Boolean IsPartialQuery { get; set; }
        public Int32 PhraseSlop { get; set; }
        public Double ScoreMultiplier { get; set; }
        public String DetectedGender { get; set; }
    }
}
