using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class UserCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public UserCensor
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

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.User));
            
            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseUsers))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }


            AuthContext shelterContext = _contextBuilder.OwnedFrom(new ShelterLookup(), context.CurrentUserId).AffiliatedWith(new ShelterLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.Shelter)), shelterContext), nameof(Models.User.User.Shelter)));


            // Create the file lookup for the profile photo censoring
            FileLookup fileLookup = new FileLookup();
            fileLookup.OwnerIds = [context.CurrentUserId];
            AuthContext fileContext = _contextBuilder.OwnedFrom(fileLookup).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.ProfilePhoto)), fileContext), nameof(Models.User.User.ProfilePhoto)));

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
