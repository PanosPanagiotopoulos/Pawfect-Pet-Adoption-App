using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.MessageServices;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly ConversationQuery _conversationQuery;
		private readonly ConversationBuilder _conversationBuilder;
		private readonly IConversationRepository _conversationRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
		private readonly MessageQuery _messageQuery;
		private readonly Lazy<IMessageService> _messageService;

		public ConversationService
		(
			ConversationQuery conversationQuery,
			ConversationBuilder conversationBuilder,
			IConversationRepository conversationRepository,
			IMapper mapper,
			IConventionService conventionService,
			MessageQuery messageQuery,
			Lazy<IMessageService> messageService
		)
		{
			_conversationQuery = conversationQuery;
			_conversationBuilder = conversationBuilder;
			_conversationRepository = conversationRepository;
			_mapper = mapper;
			_conventionService = conventionService;
			_messageQuery = messageQuery;
			_messageService = messageService;
		}

		public async Task<IEnumerable<ConversationDto>> QueryConversationsAsync(ConversationLookup conversationLookup)
		{
			List<Conversation> queriedConversations = await conversationLookup.EnrichLookup(_conversationQuery).CollectAsync();
			return await _conversationBuilder.SetLookup(conversationLookup).BuildDto(queriedConversations, conversationLookup.Fields.ToList());
		}

		public async Task<ConversationDto?> Persist(ConversationPersist persist, List<String> fields)
		{
			Boolean isUpdate = _conventionService.IsValidId(persist.Id);
			Conversation data = new Conversation();
			String dataId = String.Empty;
			if (isUpdate)
			{
				// TODO : Correct logic?
				throw new InvalidOperationException("Cannot update a conversation");
			}
			else
			{
				_mapper.Map(persist, data);
				data.Id = null; // Ensure new ID is generated
				data.CreatedAt = DateTime.UtcNow;
				data.UpdatedAt = DateTime.UtcNow;
			}

			if (isUpdate) dataId = await _conversationRepository.UpdateAsync(data);
			else dataId = await _conversationRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
			{
				throw new InvalidOperationException("Αποτυχία κατά το persist της συζήτησης");
			}

			// Return dto model
			ConversationLookup lookup = new ConversationLookup(_conversationQuery);
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

			return (
					 await _conversationBuilder.SetLookup(lookup)
					.BuildDto(await lookup.EnrichLookup().CollectAsync(), lookup.Fields.ToList())
					).FirstOrDefault();
		}

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			MessageLookup lookup = new MessageLookup(_messageQuery);
			lookup.ConversationIds = ids;
			lookup.Fields = new List<String> { nameof(MessageDto.Id) };
			lookup.Offset = 0;
			lookup.PageSize = 50;

			List<Message> messages = await lookup.EnrichLookup().CollectAsync();
			await _messageService.Value.Delete(messages?.Select(x => x.Id).ToList());

			await _conversationRepository.DeleteAsync(ids);
		}
	}
}