using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Services.AuthenticationServices;

namespace Pawfect_Messenger.Censors
{
    public class ConversationCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public ConversationCensor
        (
            IAuthorizationService authorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver
        )
        {
            _authorizationService = authorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
        }

        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<String> censoredFields = new List<String>();

            // Check if user can browse conversations
            if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseConversations))
            {
                censoredFields.AddRange(ExtractNonPrefixed(fields));
                censoredFields = [.. censoredFields.Distinct()];
            }

            // Censor Participants field (collection of users)
            AuthContext participantsContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.Participants)), participantsContext), nameof(Models.Conversation.Conversation.Participants)));

            // Censor LastMessagePreview field
            AuthContext lastMessageContext = _contextBuilder.OwnedFrom(new MessageLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<MessageCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.LastMessagePreview)), lastMessageContext), nameof(Models.Conversation.Conversation.LastMessagePreview)));

            // Censor CreatedBy field
            AuthContext createdByContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.CreatedBy)), createdByContext), nameof(Models.Conversation.Conversation.CreatedBy)));

            return censoredFields;
        }
    }
}