using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Services.AuthenticationServices;

namespace Pawfect_Messenger.Censors
{
    public class MessageCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public MessageCensor
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

            // Check if user can browse messages
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseMessages))
            {
                censoredFields.AddRange(ExtractNonPrefixed(fields));
                censoredFields = [.. censoredFields.Distinct()];
            }

            // Censor Conversation field
            AuthContext conversationContext = _contextBuilder.OwnedFrom(new ConversationLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ConversationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Conversation)), conversationContext), nameof(Models.Message.Message.Conversation)));

            // Censor Sender field
            AuthContext senderContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Sender)), senderContext), nameof(Models.Message.Message.Sender)));

            // Censor ReadBy field (collection of users)
            AuthContext readByContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.ReadBy)), readByContext), nameof(Models.Message.Message.ReadBy)));

            return censoredFields;
        }
    }
}