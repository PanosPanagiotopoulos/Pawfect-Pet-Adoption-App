using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class ConversationCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public ConversationCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Conversation));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseConversations))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext userContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.Users)), userContext), nameof(Models.Conversation.Conversation.Users)));


            AuthContext animalContext = _contextBuilder.OwnedFrom(new AnimalLookup(), context.CurrentUserId).AffiliatedWith(new AnimalLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AnimalCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.Animal)), animalContext), nameof(Models.Conversation.Conversation.Animal)));

            return censoredFields;
        }
    }
}
