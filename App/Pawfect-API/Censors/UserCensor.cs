using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;
using Microsoft.Extensions.Options;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;

namespace Pawfect_API.Censors
{
    public class UserCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;
        private readonly UserFields _userFields;

        public UserCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver,
            IOptions<UserFields> userFields
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
            _userFields = userFields.Value;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            List<String> censoredFields = new List<String>();

            Boolean isOwner = context.OwnedResource != null && await _authorizationService.AuthorizeOwnedAsync(context.OwnedResource);
            censoredFields.AddRange(this.ExtractNonPrefixed(fields, isOwner));

            if (await _authorizationService.AuthorizeAsync(Permission.BrowseUsers))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
                censoredFields = [..censoredFields.Distinct()];
            }

            AuthContext shelterContext = _contextBuilder.OwnedFrom(new ShelterLookup(), context.CurrentUserId).AffiliatedWith(new ShelterLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.Shelter)), shelterContext), nameof(Models.User.User.Shelter)));

            AuthContext adoptionApplicationContext = _contextBuilder.OwnedFrom(new AdoptionApplicationLookup(), context.CurrentUserId).AffiliatedWith(new AdoptionApplicationLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AdoptionApplicationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.RequestedAdoptionApplications)), adoptionApplicationContext), nameof(Models.User.User.RequestedAdoptionApplications)));

            // Create the file lookup for the profile photo censoring
            FileLookup fileLookup = new FileLookup();
            if (!String.IsNullOrEmpty(context.CurrentUserId))
                fileLookup.OwnerIds = [context.CurrentUserId];

            AuthContext fileContext = _contextBuilder.OwnedFrom(fileLookup, context.CurrentUserId).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.User.User.ProfilePhoto)), fileContext), nameof(Models.User.User.ProfilePhoto)));

            return censoredFields;
        }

        public List<String> ExtractNonPrefixed(List<String> fields, Boolean isOwner = false)
        {
            List<String> nonPrefixed = base.ExtractNonPrefixed(fields);
            List<String> ownerAllowedFields = isOwner ? _userFields.Owner : _userFields.External;

            return [.. nonPrefixed.Intersect(ownerAllowedFields)];
        }
            

    }
}
