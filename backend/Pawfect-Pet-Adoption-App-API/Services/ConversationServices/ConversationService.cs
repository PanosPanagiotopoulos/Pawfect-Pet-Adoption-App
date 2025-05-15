using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Builders;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.MessageServices;

namespace Pawfect_Pet_Adoption_App_API.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly IConversationRepository _conversationRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
		private readonly Lazy<IMessageService> _messageService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public ConversationService
		(
			IConversationRepository conversationRepository,
			IMapper mapper,
			IConventionService conventionService,
			Lazy<IMessageService> messageService,
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory

        )
		{
			_conversationRepository = conversationRepository;
			_mapper = mapper;
			_conventionService = conventionService;
			_messageService = messageService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
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
			ConversationLookup lookup = new ConversationLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            return (await _builderFactory.Builder<ConversationBuilder>().BuildDto(await lookup.EnrichLookup(_queryFactory).CollectAsync(), fields)).FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
			// TODO : Authorization
			MessageLookup lookup = new MessageLookup();
			lookup.ConversationIds = ids;
			lookup.Fields = new List<String> { nameof(MessageDto.Id) };
			lookup.Offset = 0;
			lookup.PageSize = 50;

			List<Message> messages = await lookup.EnrichLookup(_queryFactory).CollectAsync();
			await _messageService.Value.Delete(messages?.Select(x => x.Id).ToList());

			await _conversationRepository.DeleteAsync(ids);
		}
	}
}