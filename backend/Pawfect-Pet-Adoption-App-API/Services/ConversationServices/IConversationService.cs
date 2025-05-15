using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public interface IConversationService
	{
		Task<ConversationDto?> Persist(ConversationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}