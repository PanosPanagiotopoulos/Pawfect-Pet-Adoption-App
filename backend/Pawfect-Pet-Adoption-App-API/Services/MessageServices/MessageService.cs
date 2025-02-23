using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Services.MessageServices
{
	public class MessageService : IMessageService
	{
		private readonly MessageQuery _messageQuery;
		private readonly MessageBuilder _messageBuilder;
		private readonly IMessageRepository _messageRepository;
		private readonly IMapper _mapper;

		public MessageService
		(
			MessageQuery messageQuery,
			MessageBuilder messageBuilder,
			IMessageRepository messageRepository,
			IMapper mapper
		)
		{
			_messageQuery = messageQuery;
			_messageBuilder = messageBuilder;
			_messageRepository = messageRepository;
			_mapper = mapper;
		}

		public async Task<IEnumerable<MessageDto>> QueryMessagesAsync(MessageLookup messageLookup)
		{
			List<Message> queriedMessages = await messageLookup.EnrichLookup(_messageQuery).CollectAsync();
			return await _messageBuilder.SetLookup(messageLookup).BuildDto(queriedMessages, messageLookup.Fields.ToList());
		}

		public async Task<MessageDto?> Persist(MessagePersist persist)
		{
			Boolean isUpdate = await _messageRepository.ExistsAsync(x => x.Id == persist.Id);
			Message data = new Message();
			String dataId = String.Empty;
			if (isUpdate)
			{
				_mapper.Map(persist, data);
				dataId = await _messageRepository.UpdateAsync(data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				dataId = await _messageRepository.AddAsync(data);
			}

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist του μηνύματος");
			}

			// Return dto model
			MessageLookup lookup = new MessageLookup(_messageQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = new List<String> { "*", nameof(User) + ".*" };
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _messageBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}
	}
}