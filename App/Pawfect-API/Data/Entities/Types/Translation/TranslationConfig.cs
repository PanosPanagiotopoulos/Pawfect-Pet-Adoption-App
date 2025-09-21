namespace Pawfect_API.Data.Entities.Types.Translation
{
    public class TranslationConfig
    {
        // Configs
        public String Url { get; set; }
        public String DefaultLanguage { get; set; }

        // Processing
        public int TimeoutSeconds { get; set; }
        public Boolean EnableCaching { get; set; }
        public int MaxCacheSize { get; set; }
        public int MaxChunkSize { get; set; }

    }
}
