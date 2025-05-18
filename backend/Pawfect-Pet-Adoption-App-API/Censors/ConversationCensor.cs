using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class ConversationCensor : BaseCensor
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly ICensorFactory _censorFactory;

        public ConversationCensor
        (
            IAuthorisationService authorisationService,
            ICensorFactory censorFactory
        )
        {
            _authorisationService = authorisationService;
            _censorFactory = censorFactory;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Conversation));

            List<String> censoredFields = new List<String>();
            if (await _authorisationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseConversations))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            censoredFields.AddRange(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.Users)), context));
            censoredFields.AddRange(await _censorFactory.Censor<AnimalCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Conversation.Conversation.Animal)), context));

            return censoredFields;
        }
    }
}
