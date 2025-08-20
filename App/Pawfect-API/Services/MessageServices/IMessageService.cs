using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Message;

namespace Pawfect_API.Services.MessageServices
{
	public interface IMessageService
	{
		Task<Message?> Persist(MessagePersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}