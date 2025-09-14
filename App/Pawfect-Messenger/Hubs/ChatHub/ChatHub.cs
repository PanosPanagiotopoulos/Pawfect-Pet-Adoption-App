using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.Convention;
using Pawfect_Messenger.Services.PresenceServices;
using System.Security.Claims;

namespace Pawfect_Messenger.Hubs.ChatHub
{
    [Authorize]
    public class ChatHub : Hub<IChatClient>
    {
        public static String ConversationGroup(String conversationId) => $"conv:{conversationId}";

        private readonly ClaimsExtractor _claimsExtractor;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IPresenceService _presenceService;
        private readonly Services.AuthenticationServices.IAuthorizationService _authorizationService;
        private readonly IConventionService _conventionService;

        public ChatHub
        (
            ClaimsExtractor claimsExtractor,
            AuthContextBuilder contextBuilder,
            IPresenceService presenceService,
            Services.AuthenticationServices.IAuthorizationService authorizationService,
            IConventionService conventionService
        )
        {
            _claimsExtractor = claimsExtractor;
            _contextBuilder = contextBuilder;
            _presenceService = presenceService;
            _authorizationService = authorizationService;
            _conventionService = conventionService;
        }

        public override async Task OnConnectedAsync()
        {
            String userId = _claimsExtractor.CurrentUserId(Context.User);
            if (_conventionService.IsValidId(userId))
            {
                await _presenceService.MarkOnline(userId, Context.ConnectionId);

                Models.User.UserPresence me = await _presenceService.GetPresence(userId);

                await Clients.User(userId).PresenceChanged(me);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            String userId = _claimsExtractor.CurrentUserId(Context.User);
            if (_conventionService.IsValidId(userId))
            {
                await _presenceService.MarkOffline(userId, Context.ConnectionId);

                Models.User.UserPresence me = await _presenceService.GetPresence(userId);

                await Clients.User(userId).PresenceChanged(me);
            }
            await base.OnDisconnectedAsync(exception);
        }

        // Clients call this to start receiving a conversation's messages
        public async Task JoinConversation(String conversationId)
        {
            if (!_conventionService.IsValidId(conversationId)) throw new HubException("Invalid conversation id.");

            String userId = _claimsExtractor.CurrentUserId(Context.User);
            if (!_conventionService.IsValidId(userId)) throw new HubException("User is not authenticated.");

            ConversationLookup authLookup = new ConversationLookup { Ids = [conversationId] };
            AuthContext context = _contextBuilder.OwnedFrom(authLookup, userId).AffiliatedWith(authLookup).Build();

            if (!await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseConversations))
                throw new HubException("User is not authorized to view conversation.");

            await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));

            await Clients.Group(ConversationGroup(conversationId)).PresenceChanged(new Models.User.UserPresence()
            {
                Status = Data.Entities.EnumTypes.UserStatus.Online,
                UserId = userId
            });

        }

        public async Task LeaveConversation(String conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroup(conversationId));

            String userId = _claimsExtractor.CurrentUserId(Context.User);
            if (!_conventionService.IsValidId(userId)) throw new HubException("User is not authenticated.");

            await Clients.Group(ConversationGroup(conversationId)).PresenceChanged(new Models.User.UserPresence()
            {
                Status = Data.Entities.EnumTypes.UserStatus.Offline,
                UserId = userId
            });
        }    
            
    }
}
