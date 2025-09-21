using Pawfect_API.Data.Entities.EnumTypes;

namespace Pawfect_API.Models.AiAssistant
{
    public class CompletionsRequest
    {
        public String Prompt { get; set; }
        public String ContextAnimalId { get; set; }

        public List<AiMessage> ConversationHistory { get; set; }
    }

    public class AiMessage
    {
        public AiMessageRole Role { get; set; }
        public String Content { get; set; }
    }
}
