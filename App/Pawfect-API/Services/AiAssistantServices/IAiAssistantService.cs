using Pawfect_API.Models.AiAssistant;

namespace Pawfect_API.Services.AiAssistantServices
{
    public interface IAiAssistantService
    {
        Task<CompletionsResponse> CompleteionAsync(CompletionsRequest request);
    }
}
