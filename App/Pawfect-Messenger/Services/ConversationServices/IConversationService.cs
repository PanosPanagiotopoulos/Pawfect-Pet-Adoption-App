using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Lookups;

namespace Pawfect_Messenger.Services.ConversationServices
{
	public interface IConversationService
	{
		Task<Conversation?> Persist(ConversationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}