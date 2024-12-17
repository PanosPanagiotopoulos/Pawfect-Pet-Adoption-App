using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;

namespace Pawfect_Pet_Adoption_App_API.Services
{
    public interface IMessageService
    {
        // Συνάρτηση για query στα messages
        Task<IEnumerable<MessageDto>> QueryMessagesAsync(MessageLookup messageLookup);
    }
}