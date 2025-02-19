using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public interface IConversationService
	{
		// Συνάρτηση για query στα conversations
		Task<IEnumerable<ConversationDto>> QueryConversationsAsync(ConversationLookup conversationLookup);
	}
}