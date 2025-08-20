using Pawfect_API.Models.Conversation;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.ConversationServices
{
	public interface IConversationService
	{
		Task<Conversation?> Persist(ConversationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}