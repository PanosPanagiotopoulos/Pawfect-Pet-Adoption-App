using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class MessageCensor : BaseCensor
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly ICensorFactory _censorFactory;
        private readonly IAuthorisationContentResolver _authorisationContentResolver;
        private readonly AuthContextBuilder _authorisationContextBuilder;

        public MessageCensor
        (
            IAuthorisationService authorisationService,
            ICensorFactory censorFactory,
            IAuthorisationContentResolver authorisationContentResolver,
            AuthContextBuilder authorisationContextBuilder
        )
        {
            _authorisationService = authorisationService;
            _censorFactory = censorFactory;
            _authorisationContentResolver = authorisationContentResolver;
            _authorisationContextBuilder = authorisationContextBuilder;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Message));

            List<String> censoredFields = new List<String>();
            if (await _authorisationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseMessages))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            censoredFields.AddRange(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Recipient)), context));
            censoredFields.AddRange(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Sender)), context));

            AuthContext conversationContext = _authorisationContextBuilder
                .OwnedFrom(context.OwnedResource)
                .AffiliatedWith(new ConversationLookup(), _authorisationContentResolver.AffiliatedRolesOf(Permission.BrowseConversations))
                .Build();
            censoredFields.AddRange(await _censorFactory.Censor<ConversationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Message.Message.Conversation)), context));
            


            return censoredFields;
        }
    }
}
