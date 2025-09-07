using AutoMapper;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.MessageServices;

namespace Pawfect_Messenger.Services.ConversationServices
{
	public class ConversationService : IConversationService
	{
		private readonly IConversationRepository _conversationRepository;
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
			_conventionService = conventionService;
			_messageService = messageService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _contextBuilder = contextBuilder;
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _authorizationContentResolver = AuthorizationContentResolver;
        }

		public async Task<Models.Conversation.Conversation> Persist(ConversationPersist persist, List<String> fields)
		{
            return null;
        }

		public async Task Delete(String id) { await Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            ConversationLookup lookup = new ConversationLookup();
            lookup.Ids = ids;
            lookup.Fields = new List<String> { nameof(Models.Conversation.Conversation.Id), nameof(Models.Conversation.Conversation.Participants) + "." + nameof(Models.User.User.Id) };
            lookup.Offset = 0;
            lookup.PageSize = 1000;

            List<Data.Entities.Conversation> conversations = await lookup.EnrichLookup(_queryFactory).CollectAsync();

            OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new ConversationLookup(), [.. conversations.SelectMany(x => x.Participants)]);
            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteConversations))
                throw new ForbiddenException("You do not have permission to delete files.", typeof(Data.Entities.Conversation), Permission.DeleteConversations);

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