
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;

namespace Pawfect_Pet_Adoption_App_API.Censors
{
    public class AnimalCensor : BaseCensor
    {
        private readonly IAuthorisationService _authorisationService;
        private readonly ICensorFactory _censorFactory;

        public AnimalCensor
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

            if (fields.Contains("*")) fields = this.ExtractForeign(fields, typeof(Data.Entities.Animal));

            List<String> censoredFields = new List<String>();
            if (await _authorisationService.AuthorizeOrOwnedOrAffiliated(context, Permission.BrowseAnimals))
            {
                censoredFields.AddRange(this.ExtractNonPrefixed(fields));
            }


            censoredFields.AddRange(await _censorFactory.Censor<ShelterCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Shelter)), context));
            censoredFields.AddRange(await _censorFactory.Censor<AnimalTypeCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.AnimalType)), context));
            censoredFields.AddRange(await _censorFactory.Censor<BreedCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Breed)), context));
            censoredFields.AddRange(await _censorFactory.Censor<FileCensor>().Censor(this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Photos)), context));

            return censoredFields;
        }

        public List<String> PseudoCensor(List<String> fields)
        {
            if (fields == null || fields.Count == 0) return new List<String>();

            List<String> nonPrefixed = this.ExtractNonPrefixed(fields);
            List<String> animalTypeFields = this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.AnimalType));
            List<String> breedFields = this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Breed));
            List<String> photoFields = [..this.ExtractPrefixed(fields, nameof(Models.Animal.Animal.Photos))
                                       .Where(field => field.Equals(nameof(Models.File.File.SourceUrl), StringComparison.OrdinalIgnoreCase))];


            return [..nonPrefixed.Concat(animalTypeFields).Concat(breedFields).Concat(photoFields)];
        }
    }
}
