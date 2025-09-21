namespace Pawfect_API.Data.Entities.Types.AiAssistant
{
    public class AiAssistantConfig
    {
        public String ApiKey { get; set; }
        public String InstructionsPath { get; set; }
        public String RagContextPlaceholder { get; set; }
        public int RagBatchSize { get; set; }
        public List<String> ContextFields { get; set; }
    }
}
