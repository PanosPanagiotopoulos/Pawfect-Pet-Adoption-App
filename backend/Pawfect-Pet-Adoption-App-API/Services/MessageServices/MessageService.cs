using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Services.MessageServices
{
	public class MessageService : IMessageService
	{
		private readonly MessageQuery _messageQuery;
		private readonly MessageBuilder _messageBuilder;

		public MessageService(MessageQuery messageQuery, MessageBuilder messageBuilder)
		{
			_messageQuery = messageQuery;
			_messageBuilder = messageBuilder;
		}

		public async Task<IEnumerable<MessageDto>> QueryMessagesAsync(MessageLookup messageLookup)
		{
			List<Message> queriedMessages = await messageLookup.EnrichLookup(_messageQuery).CollectAsync();
			return await _messageBuilder.SetLookup(messageLookup).BuildDto(queriedMessages, messageLookup.Fields.ToList());
		}
	}
}