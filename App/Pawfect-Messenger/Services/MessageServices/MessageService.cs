using AutoMapper;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Models.Message;
using Pawfect_Messenger.Query;
using Pawfect_Messenger.Query.Interfaces;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using System.Security.Claims;

namespace Pawfect_Messenger.Services.MessageServices
{
	public class MessageService : IMessageService
	{
		private readonly IMessageRepository _messageRepository;
		private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public MessageService
		(
			IMessageRepository messageRepository,
			IConventionService conventionService,
			IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver AuthorizationContentResolver,
			ClaimsExtractor claimsExtractor,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder
        )
		{
			_messageRepository = messageRepository;
			_conventionService = conventionService;
            _queryFactory = queryFactory;
            _builderFactory = builderFactory;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }

		public async Task<Models.Message.Message> Persist(MessagePersist persist, List<String> fields)
		{
            if (!await this.AuthoriseMessagePersist(persist, Permission.EditMessages))
                throw new ForbiddenException("You do not have permission to create messages.", typeof(Data.Entities.Message), Permission.EditMessages);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Message data = new Data.Entities.Message();
            String dataId = String.Empty;
			if (isUpdate)
			{
				data = await _messageRepository.FindAsync(x => x.Id == persist.Id);
				if (data == null) throw new NotFoundException("No message found with this id", persist.Id, typeof(Data.Entities.Message));
			}
			else
			{
				data.Id = null; // Ensure new ID is generated
                data.Type = persist.Type.Value;
                data.SenderId = persist.SenderId;
                data.ConversationId = persist.ConversationId;
                data.UpdatedAt = DateTime.UtcNow;
                data.Status = Data.Entities.EnumTypes.MessageStatus.Sending;
				data.ReadBy = [];
				data.CreatedAt = DateTime.UtcNow;
			}

            data.Content = persist.Content;

			if (isUpdate) dataId = await _messageRepository.UpdateAsync(data);
			else dataId = await _messageRepository.AddAsync(data);

			if (String.IsNullOrEmpty(dataId))
				throw new InvalidOperationException("Failed to persist message");

			// Return dto model
			MessageLookup lookup = new MessageLookup();
			lookup.Ids = new List<String> { dataId };
			lookup.Fields = fields;
			lookup.Offset = 0;
			lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying notifications");

            lookup.Fields = censoredFields;
            return (await _builderFactory.Builder<MessageBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
										 .Build(await lookup.EnrichLookup(_queryFactory).Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).CollectAsync(), censoredFields))
										 .FirstOrDefault();
        }

		private async Task<Boolean> AuthoriseMessagePersist(MessagePersist messagePersist, String permission)
		{
			ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No authenticated user found");

			if (messagePersist.SenderId != userId) return false;

            ConversationLookup lookup = new ConversationLookup();
            lookup.Participants = new List<String> { messagePersist.SenderId };

            AffiliatedResource resource = _authorizationContentResolver.BuildAffiliatedResource(lookup, permission);
			return await _authorizationService.AuthorizeOrAffiliatedAsync(resource, permission);
        }

		public async Task Delete(String id) { await Delete(new List<String>() { id }); }

		public async Task Delete(List<String> ids)
		{
            MessageLookup lookup = new MessageLookup();
            lookup.Ids = ids;
            lookup.Offset = 0;
            lookup.PageSize = 10000;
            lookup.Fields = new List<String> {
                                                nameof(Models.Message.Message.Id),
                                                nameof(Models.Message.Message.Sender) + "." + nameof(Models.User.User.Id),
                                             };

            List<Data.Entities.Message> datas = await lookup.EnrichLookup(_queryFactory).CollectAsync();

			OwnedResource ownedResource = _authorizationContentResolver.BuildOwnedResource(new MessageLookup(), [..datas.Select(x => x.SenderId)]);
            if (!await _authorizationService.AuthorizeOrOwnedAsync(ownedResource, Permission.DeleteMessages))
                throw new ForbiddenException("You do not have permission to delete messages.", typeof(Data.Entities.Message), Permission.DeleteMessages);


            await _messageRepository.DeleteManyAsync(ids);
		}
	}
}