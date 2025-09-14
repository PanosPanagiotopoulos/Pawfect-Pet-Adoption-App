using Microsoft.AspNetCore.SignalR;
using Pawfect_Messenger.Builders;
using Pawfect_Messenger.Censors;
using Pawfect_Messenger.Data.Entities;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Exceptions;
using Pawfect_Messenger.Hubs.ChatHub;
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
        private readonly ILogger<MessageService> _logger;
        private readonly IBuilderFactory _builderFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly ClaimsExtractor _claimsExtractor;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IConversationRepository _conversationRepository;
        private readonly IHubContext<ChatHub, IChatClient> _chatHub;

        public MessageService
		(
			IMessageRepository messageRepository,
			IConventionService conventionService,
			IQueryFactory queryFactory,
            ILogger<MessageService> logger,
            IBuilderFactory builderFactory,
			IAuthorizationService AuthorizationService,
			IAuthorizationContentResolver AuthorizationContentResolver,
			ClaimsExtractor claimsExtractor,
			ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IConversationRepository conversationRepository,
            IHubContext<ChatHub, IChatClient> chatHub
        )
		{
			_messageRepository = messageRepository;
			_conventionService = conventionService;
            _queryFactory = queryFactory;
            _logger = logger;
            _builderFactory = builderFactory;
            _authorizationService = AuthorizationService;
            _authorizationContentResolver = AuthorizationContentResolver;
            _claimsExtractor = claimsExtractor;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _conversationRepository = conversationRepository;
            _chatHub = chatHub;
        }

        public async Task<Models.Message.Message> Persist(MessagePersist persist, List<String> fields = null)
        {
            if (!await this.AuthoriseMessagePersist(persist, Permission.EditMessages))
                throw new ForbiddenException("You do not have permission to create messages.", typeof(Data.Entities.Message), Permission.EditMessages);

            Boolean isUpdate = _conventionService.IsValidId(persist.Id);
            Data.Entities.Message data = new Data.Entities.Message();
            String dataId = String.Empty;
            if (isUpdate)
            {
                data = await _messageRepository.FindAsync(x => x.Id == persist.Id && x.Status != Data.Entities.EnumTypes.MessageStatus.Failed);
                if (data == null) throw new NotFoundException("No message found with this id", persist.Id, typeof(Data.Entities.Message));
            }
            else
            {
                data.Id = null; // Ensure new ID is generated
                data.Type = persist.Type.Value;
                data.SenderId = persist.SenderId;
                data.ConversationId = persist.ConversationId;
                data.UpdatedAt = DateTime.UtcNow;
                data.ReadBy = [persist.SenderId];
                data.CreatedAt = DateTime.UtcNow;
            }

            data.Content = persist.Content;
            data.Status = Data.Entities.EnumTypes.MessageStatus.Sending;
            try
            {
                if (isUpdate) data.Id = await _messageRepository.UpdateAsync(data);
                else data.Id = await _messageRepository.AddAsync(data);

                data.Status = Data.Entities.EnumTypes.MessageStatus.Delivered;

                await _messageRepository.UpdateAsync(data);
            }
            // Mark as error in excpetion
            catch (Exception ex) 
            { 
                _logger.LogWarning("Failed to persist message setting it to error sent");
               
                data.Status = Data.Entities.EnumTypes.MessageStatus.Failed;

                if (isUpdate) data.Id = await _messageRepository.UpdateAsync(data);
                else data.Id = await _messageRepository.AddAsync(data);
            }

            if (String.IsNullOrEmpty(data.Id)) throw new InvalidOperationException("Failed to persist message");

            // Return dto model
            MessageLookup lookup = new MessageLookup();
            lookup.Ids = new List<String> { data.Id };
            lookup.Fields = fields ?? CommonFieldsHelper.MessageFields();
            lookup.Offset = 0;
            lookup.PageSize = 1;

            AuthContext context = _contextBuilder.OwnedFrom(lookup).Build();
            List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor([.. lookup.Fields], context);
            if (censoredFields.Count == 0) throw new ForbiddenException("Unauthorised access when querying messages");
            lookup.Fields = censoredFields;

            Models.Message.Message message = (await _builderFactory.Builder<MessageBuilder>().Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation).Build([data], censoredFields)).FirstOrDefault();

            Data.Entities.Conversation conversation = await _conversationRepository.FindAsync(x => x.Id == data.ConversationId);
            if (message.Status == Data.Entities.EnumTypes.MessageStatus.Delivered)
            {
                try
                {
                    await _chatHub.Clients.Users(conversation.Participants).MessageReceived(message);
                }
                // Ignore in exception
                catch (Exception ex) { _logger.LogWarning("Failed to push the message creation event to the client connection"); }
            }

            try
            {
                await _chatHub.Clients.Users(conversation.Participants).MessageStatusChanged(message);
            }

            // Ignore in exception
            catch (Exception ex) { _logger.LogWarning("Failed to push the message status changed event to the client connection"); }


            if (message.Status != Data.Entities.EnumTypes.MessageStatus.Failed)
            {
                if (conversation != null)
                {
                    conversation.LastMessageAt = data.CreatedAt;
                    conversation.LastMessageId = data.Id;
                    if (String.IsNullOrEmpty(await _conversationRepository.UpdateAsync(conversation)))
                        throw new InvalidOperationException("Failed to update conversation with last message info");

                    await _chatHub.Clients.Users(conversation.Participants).LastConversationMessageUpdated(message);
                }
            }


            return message;
        }

        public async Task MarkRead(List<MessageReadPersist> models, List<String> fields = null)
        {
            if (models == null || models.Count == 0) return;

            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No authenticated user found");

            // Default requested fields if none provided
            fields ??= CommonFieldsHelper.MessageFields();

            // Distinct, valid ids only
            List<String> allIds = models
                .Select(m => m.MessageId)
                .Distinct()
                .ToList();

            const int BATCH_SIZE = 8;

            for (int start = 0; start < allIds.Count; start += BATCH_SIZE)
            {
                List<String> batchIds = allIds.Skip(start).Take(BATCH_SIZE).ToList();
                if (batchIds.Count == 0) continue;

                // --- Load messages for this batch (exclude Failed) ---
                MessageLookup loadLookup = new MessageLookup
                {
                    Ids = batchIds,
                    Statuses = new List<MessageStatus> { MessageStatus.Sending, MessageStatus.Delivered },
                    Offset = 0,
                    PageSize = batchIds.Count,
                };

                List<Data.Entities.Message> entities = await loadLookup.EnrichLookup(_queryFactory).CollectAsync();
                if (entities == null || entities.Count == 0) continue;

                List<String> singleConversationIdList = entities.Select(x => x.ConversationId).Distinct().ToList();
                if (singleConversationIdList.Count != 1)
                    throw new ForbiddenException("All messages should be from the same conversation");

                MessagePersist persistForAuth = new MessagePersist { ConversationId = singleConversationIdList[0] };
                if (!await this.AuthoriseMessageReadPersist(persistForAuth, Permission.EditMessages)) throw new ForbiddenException("You do not have permission to mark messages as read.", typeof(Data.Entities.Message), Permission.EditMessages);


                // --- Add userId to ReadBy only if not already present ---
                List<Data.Entities.Message> toUpdate = entities.Where(m => m.ReadBy == null || !m.ReadBy.Contains(userId)).ToList();
                if (toUpdate.Count == 0) continue;

                foreach (Data.Entities.Message d in toUpdate)
                {
                    d.ReadBy ??= new List<String>();
                    d.ReadBy.Add(userId);
                }

                String[] updateResult = await Task.WhenAll(toUpdate.Select(m => _messageRepository.UpdateAsync(m)));

                if (updateResult == null || updateResult.Length != toUpdate.Count) throw new InvalidOperationException("Failed to read messages");

                MessageLookup buildLookup = new MessageLookup
                {
                    Ids = toUpdate.Select(m => m.Id).ToList(),
                    Fields = fields,
                    Offset = 0,
                    PageSize = toUpdate.Count
                };

                AuthContext context = _contextBuilder.OwnedFrom(buildLookup).AffiliatedWith(buildLookup).Build();
                List<String> censoredFields = await _censorFactory.Censor<MessageCensor>().Censor([.. fields], context);
                if (censoredFields.Count == 0)
                    throw new ForbiddenException("Unauthorised access when querying messages");

                List<Models.Message.Message> readDtos =
                    await _builderFactory
                        .Builder<MessageBuilder>()
                        .Authorise(AuthorizationFlags.OwnerOrPermissionOrAffiliation)
                        .Build(toUpdate, censoredFields);

                Dictionary<String, String> senderById = toUpdate.ToDictionary(m => m.Id, m => m.SenderId);
                List<String> convParticipants = (await _conversationRepository.FindAsync(x => x.Id == singleConversationIdList[0], [nameof(Data.Entities.Conversation.Participants)])).Participants;

                foreach (Models.Message.Message dto in readDtos)
                {
                    if (dto == null) continue;
                    if (!senderById.TryGetValue(dto.Id, out String senderId) || !_conventionService.IsValidId(senderId)) continue;

                    try
                    {
                        await Task.WhenAll(
                            _chatHub.Clients.User(senderId).MessageRead(dto),
                            _chatHub.Clients.Users(convParticipants).LastConversationMessageUpdated(dto)
                        );
                       
                    }
                    catch {}
                }
            }
        }

        private async Task<Boolean> AuthoriseMessagePersist(MessagePersist messagePersist, String permission)
		{
			ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No authenticated user found");

			if (messagePersist.SenderId != userId) return false;

            ConversationLookup lookup = new ConversationLookup();
            lookup.Participants = new List<String> { messagePersist.SenderId };
            lookup.Ids = [messagePersist.ConversationId];

            AffiliatedResource resource = _authorizationContentResolver.BuildAffiliatedResource(lookup, permission);
			return await _authorizationService.AuthorizeOrAffiliatedAsync(resource, permission);
        }

        private async Task<Boolean> AuthoriseMessageReadPersist(MessagePersist messagePersist, String permission)
        {
            ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
            String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
            if (!_conventionService.IsValidId(userId)) throw new Exception("No authenticated user found");

            ConversationLookup lookup = new ConversationLookup();
            lookup.Participants = new List<String> { userId };
            lookup.Ids = [messagePersist.ConversationId];

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