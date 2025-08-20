using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;

namespace Pawfect_API.Censors
{
    public class AdoptionApplicationCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public AdoptionApplicationCensor
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

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseAdoptionApplications))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext userContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.AdoptionApplication.AdoptionApplication.User)), userContext), nameof(Models.AdoptionApplication.AdoptionApplication.User)));

            AuthContext animalContext = _contextBuilder.OwnedFrom(new AnimalLookup(), context.CurrentUserId).AffiliatedWith(new AnimalLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AnimalCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.AdoptionApplication.AdoptionApplication.Animal)), animalContext), nameof(Models.AdoptionApplication.AdoptionApplication.Animal)));

            AuthContext fileContext = _contextBuilder.OwnedFrom(new FileLookup(), context.CurrentUserId).AffiliatedWith(new FileLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles)), fileContext), nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles)));

            AuthContext shelterContext = _contextBuilder.OwnedFrom(new ShelterLookup(), context.CurrentUserId).AffiliatedWith(new ShelterLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.AdoptionApplication.AdoptionApplication.Shelter)), shelterContext), nameof(Models.AdoptionApplication.AdoptionApplication.Shelter)));

            return censoredFields;
        }
    }
}
