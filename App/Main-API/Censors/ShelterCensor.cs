using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Services.AuthenticationServices;

namespace Main_API.Censors
{
    public class ShelterCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;
        private readonly IAuthorizationContentResolver _authorizationContentResolver;

        public ShelterCensor
        (
            IAuthorizationService AuthorizationService,
            ICensorFactory censorFactory,
            AuthContextBuilder contextBuilder,
            IAuthorizationContentResolver authorizationContentResolver
        )
        {
            _authorizationService = AuthorizationService;
            _censorFactory = censorFactory;
            _contextBuilder = contextBuilder;
            _authorizationContentResolver = authorizationContentResolver;
        }
        public override async Task<List<String>> Censor(List<String> fields, AuthContext context)
        {
            if (fields == null || fields.Count == 0) return new List<String>();
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Shelter));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseShelters))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext userContext = _contextBuilder.OwnedFrom(new UserLookup(), context.CurrentUserId).AffiliatedWith(new UserLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<UserCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Shelter.Shelter.User)), userContext), nameof(Models.Shelter.Shelter.User)));

            AuthContext animalContext = _contextBuilder.OwnedFrom(new AnimalLookup(), context.CurrentUserId).AffiliatedWith(new AnimalLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AnimalCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Shelter.Shelter.Animals)), animalContext), nameof(Models.Shelter.Shelter.Animals)));


            String shelterId = await _authorizationContentResolver.CurrentPrincipalShelter();
            AdoptionApplicationLookup adoptionApplicationLookup = new AdoptionApplicationLookup()
            {
                ShelterIds = !String.IsNullOrWhiteSpace(shelterId) ? [shelterId] : null
            };
            AuthContext adoptionApplicationContext = _contextBuilder.OwnedFrom(new AdoptionApplicationLookup(), context.CurrentUserId).AffiliatedWith(adoptionApplicationLookup).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AdoptionApplicationCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Shelter.Shelter.ReceivedAdoptionApplications)), adoptionApplicationContext), nameof(Models.Shelter.Shelter.ReceivedAdoptionApplications)));
            return censoredFields;
        }

        public List<String> PseudoCensor(List<String> fields)
        {
            if (fields == null || fields.Count == 0) return new List<String>();

            List<String> nonPrefixed = [..this.ExtractNonPrefixed(fields).Where(field => field.Equals(nameof(Models.Shelter.Shelter.ShelterName)))];
            List<String> userFields = _censorFactory.Censor<UserCensor>().PseudoCensor(this.ExtractPrefixed(fields, nameof(Models.Shelter.Shelter.User)));

            return [.. nonPrefixed.Concat(this.AsPrefixed(userFields, nameof(Models.Shelter.Shelter.User)))];
        }
    }
}
