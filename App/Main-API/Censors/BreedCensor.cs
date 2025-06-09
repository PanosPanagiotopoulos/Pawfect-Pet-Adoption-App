using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class BreedCensor : BaseCensor
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICensorFactory _censorFactory;
        private readonly AuthContextBuilder _contextBuilder;

        public BreedCensor
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

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Breed));

            List<String> censoredFields = new List<String>();
            if (await _authorizationService.AuthorizeAsync(Permission.BrowseBreeds))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }

            AuthContext animalTypeContext = _contextBuilder.OwnedFrom(new AnimalTypeLookup(), context.CurrentUserId).AffiliatedWith(new AnimalTypeLookup()).Build();
            censoredFields.AddRange(this.AsPrefixed(await _censorFactory.Censor<AnimalTypeCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Breed.Breed.AnimalType)), animalTypeContext), nameof(Models.Breed.Breed.AnimalType)));


            return censoredFields;
        }
    }
}
