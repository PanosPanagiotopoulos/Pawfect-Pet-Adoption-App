using Main_API.Models.Lookups;
using Main_API.Models.Message;

namespace Main_API.Services.MessageServices
{
	public interface IMessageService
	{
		Task<Message?> Persist(MessagePersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}