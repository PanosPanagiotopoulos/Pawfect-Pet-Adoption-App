using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;

namespace Pawfect_Pet_Adoption_App_API.Services.MessageServices
{
	public class MessageService : IMessageService
	{
		private readonly MessageQuery _messageQuery;
		private readonly MessageBuilder _messageBuilder;
		private readonly IMessageRepository _messageRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;

		public MessageService
		(
			MessageQuery messageQuery,
			MessageBuilder messageBuilder,
			IMessageRepository messageRepository,
			IMapper mapper,
			IConventionService conventionService
		)
		{
			_messageQuery = messageQuery;
			_messageBuilder = messageBuilder;
			_messageRepository = messageRepository;
			_mapper = mapper;
			_conventionService = conventionService;
		}

		public async Task<IEnumerable<MessageDto>> QueryMessagesAsync(MessageLookup messageLookup)
		{
			List<Message> queriedMessages = await messageLookup.EnrichLookup(_messageQuery).CollectAsync();
			return await _messageBuilder.SetLookup(messageLookup).BuildDto(queriedMessages, messageLookup.Fields.ToList());
		}

		public async Task<MessageDto?> Persist(MessagePersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Message data = new Message();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _messageRepository.FindAsync(x => x.Id == persist.Id);

				if (data == null) throw new InvalidDataException("No entity found with id given");

				_mapper.Map(persist, data);
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _messageRepository.UpdateAsync(data);
			else dataId = await _messageRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist του μηνύματος");
			}

			// Return dto model
			MessageLookup lookup = new MessageLookup(_messageQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _messageBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			await _messageRepository.DeleteAsync(ids);
		}
	}
}