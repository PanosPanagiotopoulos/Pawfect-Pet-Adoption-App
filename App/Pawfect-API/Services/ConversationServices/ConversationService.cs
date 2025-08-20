using AutoMapper;

using Pawfect_API.Builders;
using Pawfect_API.Censors;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Exceptions;
using Pawfect_API.Models.Conversation;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Query;
using Pawfect_API.Repositories.Interfaces;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.MessageServices;

namespace Pawfect_API.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly IConversationRepository _conversationRepository;
		private readonly IMapper _mapper;
		private readonly IConventionService _conventionService;
		private readonly Lazy<IMessageService> _messageService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public ConversationService
		(
			IConversationRepository conversationRepository,
			IMapper mapper,
			IConventionService conventionService,
			Lazy<IMessageService> messageService,
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			AuthContextBuilder contextBuilder,
            IAuthorizationService AuthorizationService,
			ICensorFactory censorFactory,
            IAuthorizationContentResolver AuthorizationContentResolver
        )
		{
			_conversationRepository = conversationRepository;
			_mapper = mapper;
			_conventionService = conventionService;
			_messageService = messageService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _authorizationContentResolver = AuthorizationContentResolver;
        }

		public async Task<Models.Conversation.Conversation?> Persist(ConversationPersist persist, List<String> fields)
		{
			if (!await _authorizationService.AuthorizeAsync(Permission.CreateConversations))
                throw new ForbiddenException("Unauthorised access when persisting conversations", typeof(Data.Entities.AdoptionApplication), Permission.CreateConversations);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Conversation data = new Data.Entities.Conversation();
			String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _conversationRepository.FindAsync(conv => conv.Id == persist.Id);
				if (data == null) throw new NotFoundException("Conversation not found", persist.Id, typeof(Data.Entities.Conversation));

				if (persist.UserIds.Except(data.UserIds).ToList().Count == 0) throw new InvalidOperationException("Cannot change who are in the conversation");

				_mapper.Map(persist, data);
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

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ConversationCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying conversations");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<ConversationBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										 .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
										 .FirstOrDefault();
        }

		public async Task Delete(String id) { await this.Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            ConversationLookup lookup = new ConversationLookup();
            lookup.Ids = ids;
            lookup.Fields = new List<String> { nameof(Models.Conversation.Conversation.Id), nameof(Models.Conversation.Conversation.Users) + "." + nameof(Models.User.User.Id) };
            lookup.Offset = 0;
            lookup.PageSize = 1000;

            List<Data.Entities.Conversation> conversations = await lookup.EnrichLookup(_queryFactory).CollectAsync();

            OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new ConversationLookup(), [.. conversations.SelectMany(x => x.UserIds)]);
            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteConversations))
                throw new ForbiddenException("You do not have permission to delete files.", typeof(Data.Entities.Conversation), Permission.DeleteConversations);

            // TODO : Authorization
            MessageLookup mLookup = new MessageLookup();
            mLookup.ConversationIds = ids;
            mLookup.Fields = new List<String> { nameof(Models.Message.Message.Id) };
            mLookup.Offset = 0;
            mLookup.PageSize = 50;

			List<Data.Entities.Message> messages = await mLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _messageService.Value.Delete([..messages?.Select(x => x.Id)]);

			await _conversationRepository.DeleteManyAsync(ids);
		}
	}
}