using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class UserCensor : BaseCensor
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly ICensorFactory _censorFactory;

        public UserCensor
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

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.User));
            
            List<String> censoredFields = new List<String>();
            if (await _authorisationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseUsers))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            censoredFields.AddRange(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.Shelter)), context));
            censoredFields.AddRange(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.ProfilePhoto)), context));

            return censoredFields;
        }

        public List<String> PseudoCensor(List<String> fields)
        {
            if (fields == null || fields.Count == 0) return new List<String>();

            List<String> nonPrefixed = [.. this.ExtractNonPrefixed(fields).Where(field => field.Equals(nameof(Models.User.User.Location)))];

            return nonPrefixed;
        }
    }
}
