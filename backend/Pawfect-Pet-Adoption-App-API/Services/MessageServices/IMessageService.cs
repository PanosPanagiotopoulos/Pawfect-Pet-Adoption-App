using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;

namespace Pawfect_Pet_Adoption_App_API.Services.MessageServices
{
	public interface IMessageService
	{
		Task<MessageDto?> Persist(MessagePersist persist, List<String> fields);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}