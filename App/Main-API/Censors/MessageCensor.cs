using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Services.AuthenticationServices;

namespace Main_API.Censors
{
    public class MessageCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly AuthContextBuilder _contextBuilder;

        public MessageCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            IAuthorizationContentResolver AuthorizationContentResolver,
            AuthContextBuilder AuthorizationContextBuilder
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _authorizationContentResolver = AuthorizationContentResolver;
            _contextBuilder = AuthorizationContextBuilder;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Message));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseMessages))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext recipientContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Recipient)), recipientContext), nameof(Models.Message.Message.Recipient)));

            AuthContext senderContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Sender)), senderContext), nameof(Models.Message.Message.Sender)));


            // Prepare the conversation lookup for the conversation censoring
            ConversationLookup conversationLookup = new ConversationLookup();
            conversationLookup.UserIds = [context.CurrentUserId];
            conversationLookup.Ids = [];
            MessageLookup ownedMessageLookup = (MessageLookup)context.OwnedResource?.OwnedFilterParams?.RequestedFilters;
            MessageLookup affiliatedMessageLookup = (MessageLookup)context.AffiliatedResource?.AffiliatedFilterParams?.RequestedFilters;
            if (ownedMessageLookup != null)
            {
                conversationLookup.Ids.AddRange(ownedMessageLookup.ConversationIds ?? []);
                conversationLookup.UserIds.AddRange(ownedMessageLookup.SenderIds ?? []);
                conversationLookup.UserIds.AddRange(ownedMessageLookup.RecipientIds ?? []);
            }
            if (affiliatedMessageLookup != null)
            {
                conversationLookup.Ids.AddRange(affiliatedMessageLookup.ConversationIds ?? []);
                conversationLookup.UserIds.AddRange(affiliatedMessageLookup.SenderIds ?? []);
                conversationLookup.UserIds.AddRange(affiliatedMessageLookup.RecipientIds ?? []);
            }

            conversationLookup.Ids = [.. conversationLookup.Ids.Distinct()];
            conversationLookup.UserIds = [..conversationLookup.UserIds.Distinct()];

            AuthContext convContext = _contextBuilder
                .OwnedFrom(conversationLookup)
                .AffiliatedWith(conversationLookup)
                .Build();

            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ConversationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Conversation)), convContext), nameof(Models.Message.Message.Conversation)));

            return censoredFields;
        }
    }
}
