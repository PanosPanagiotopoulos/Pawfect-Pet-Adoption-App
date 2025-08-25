
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Services.AuthenticationServices;

namespace Pawfect_API.Censors
{
    public class AnimalCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public AnimalCensor
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
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseAnimals))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext shelterContext = _contextBuilder.OwnedFrom(new ShelterLookup(), context.CurrentUserId).AffiliatedWith(new ShelterLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Shelter)), shelterContext), nameof(Models.Animal.Animal.Shelter)));
           
            
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AnimalTypeCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.AnimalType)), context), nameof(Models.Animal.Animal.AnimalType)));

            AuthContext breedContext = _contextBuilder.OwnedFrom(new BreedLookup(), context.CurrentUserId).AffiliatedWith(new BreedLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<BreedCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Breed)), breedContext), nameof(Models.Animal.Animal.Breed)));

            AuthContext fileContext = _contextBuilder.OwnedFrom(new AnimalLookup()).AffiliatedWith(new FileLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.AttachedPhotos)), fileContext), nameof(Models.Animal.Animal.AttachedPhotos)));

            return censoredFields;
        }
    }
}
