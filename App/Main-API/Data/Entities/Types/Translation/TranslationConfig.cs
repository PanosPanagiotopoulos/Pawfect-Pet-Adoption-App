namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Translation
{
    public class TranslationConfig
    {
        public String Url { get; set; }
        public String DefaultLanguage { get; set; }
        public int TimeoutSeconds { get; set; }
        public Boolean EnableCaching { get; set; }
        public int MaxCacheSize { get; set; }
    }
}
