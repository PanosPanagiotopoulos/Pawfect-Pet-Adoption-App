using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Lookups;

namespace Pawfect_Messenger.Services.ConversationServices
{
	public interface IConversationService
	{
		Task<Models.Conversation.Conversation> CreateAsync(ConversationPersist model, List<String> fields = null);

        Task Delete(String id);
		Task Delete(List<String> ids);
	}
}