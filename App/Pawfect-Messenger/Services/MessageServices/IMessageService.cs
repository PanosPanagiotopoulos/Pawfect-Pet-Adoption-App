using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Models.Message;

namespace Pawfect_Messenger.Services.MessageServices
{
	public interface IMessageService
	{
		Task<Message?> Persist(MessagePersist persist, List<String> fields = null);
		Task Delete(String id);
		Task Delete(List<String> ids);
        Task MarkRead(List<MessageReadPersist> models, List<String> fields =  null);
    }
}