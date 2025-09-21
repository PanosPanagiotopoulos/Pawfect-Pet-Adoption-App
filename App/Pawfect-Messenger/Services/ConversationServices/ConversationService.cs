using AutoMapper;
using MongoDB.Bson.Serialization.Conventions;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Conversation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Query.Queries;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.MessageServices;
using System.Reflection;
using System.Security.Claims;

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
        private readonly ClaimsExtractor _claimsExtractor;

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
            IAuthorizationContentResolver AuthorizationContentResolver,
            ClaimsExtractor claimsExtractor
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
            _claimsExtractor = claimsExtractor;
        }

        public async Task<Models.Conversation.Conversation> CreateAsync(ConversationPersist model, List<String> fields = null)
        {
            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new UnauthorizedAccessException("No authenticated user.");

            if (!await _authorizationService.AuthorizeAsync(Permission.CreateConversations)) throw new ForbiddenException();

            // Ensure the creator is in the list
            if (!model.Participants.Contains(userId)) model.Participants.Add(userId);

            // Deduplicate & sanitize
            model.Participants = model.Participants.Distinct().ToList();

            // Apply create conversation user role breakdown rules
            await this.ApplyParticipantRules(model.Participants);

            DateTime now = DateTime.UtcNow;
            Data.Entities.Conversation data = new Data.Entities.Conversation
            {
                Id = null,
                Type = model.Type.Value,
                Participants = model.Participants,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastMessageAt = null,
                LastMessageId = null,
            };

            data.Id = await _conversationRepository.AddAsync(data);


            // Return dto model
            ConversationLookup lookup = new ConversationLookup();
            lookup.Ids = [data.Id];
            lookup.Fields = fields.Concat(CommonFieldsHelper.ConversationFields()).Distinct().ToList();
            lookup.Offset = 0;
            lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).AffiliatedWith(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<ConversationCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying animals");


            return (await _builderFactory.Builder<ConversationBuilder>().Build([data], censoredFields)).FirstOrDefault();
        }

        // Apply participant rules
        // Rules: A conversation must be between: 1 User , 1 Shelter
        private async Task ApplyParticipantRules(List<String> participants)
        {
            if (participants.Count < 2) throw new InvalidOperationException("A conversation must have at least 2 participants.");
            UserQuery userQuery = _queryFactory.Query<UserQuery>();
            userQuery.Ids = participants;
            userQuery.Fields = new List<String> { nameof(Data.Entities.User.Id), nameof(Data.Entities.User.ShelterId) };
            userQuery.Offset = 0;
            userQuery.PageSize = 2;

            List<Data.Entities.User> users = await userQuery.CollectAsync();
            
            int userCount = users.Count(u => String.IsNullOrEmpty(u.ShelterId));
            int shelterCount = users.Count - userCount;

            if (userCount != 1) throw new InvalidOperationException("A conversation must have only one User.");
            if (shelterCount != 1) throw new InvalidOperationException("A conversation must have only one Shelter.");
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
            mLookup.PageSize = 100000;

			List<Data.Entities.Message> messages = await mLookup.EnrichLookup(_queryFactory).CollectAsync();
			await _messageService.Value.Delete([..messages?.Select(x => x.Id)]);

			await _conversationRepository.DeleteManyAsync(ids);
		}
	}
}