using Main_API.Models.Conversation;
using Main_API.Models.Lookups;

namespace Main_API.Services.ConversationServices
{
	public interface IConversationService
	{
		Task<Conversation?> Persist(ConversationPersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}