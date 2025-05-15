using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoShelterBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Shelter
		public AutoShelterBuilder()
		{
			// Mapping για nested object : OpeningHours
			CreateMap<OperatingHours, OperatingHours>();
			// Mapping για nested object : SocialMedia
			CreateMap<SocialMedia, SocialMedia>();

			// Mapping για το Entity : Shelter σε Shelter για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Shelter, Shelter>();

			// POST Request Dto Μοντέλα
			CreateMap<Shelter, ShelterPersist>();
			CreateMap<ShelterPersist, Shelter>();
		}
	}

	public class ShelterBuilder : BaseBuilder<ShelterDto, Shelter>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ShelterBuilder
		(
		  IQueryFactory queryFactory,
          IBuilderFactory builderFactory

        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public ShelterBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }


        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<ShelterDto>> BuildDto(List<Shelter> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, UserDto>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			Dictionary<String, List<AnimalDto>>? animalsMap = foreignEntitiesFields.ContainsKey(nameof(Animal))
				? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Animal)]))
				: null;

			List<ShelterDto> result = new List<ShelterDto>();
			foreach (Shelter e in entities)
			{
				ShelterDto dto = new ShelterDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Shelter.ShelterName))) dto.ShelterName = e.ShelterName;
				if (nativeFields.Contains(nameof(Shelter.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Shelter.Website))) dto.Website = e.Website;
				if (nativeFields.Contains(nameof(Shelter.SocialMedia))) dto.SocialMedia = e.SocialMedia;
				if (nativeFields.Contains(nameof(Shelter.OperatingHours))) dto.OperatingHours = e.OperatingHours;
				if (nativeFields.Contains(nameof(Shelter.VerificationStatus))) dto.VerificationStatus = e.VerificationStatus;
				if (nativeFields.Contains(nameof(Shelter.VerifiedBy))) dto.VerifiedBy = e.VerifiedBy;
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];
				if (animalsMap != null && animalsMap.ContainsKey(e.Id)) dto.Animals = animalsMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, UserDto>?> CollectUsers(List<Shelter> shelters, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = shelters.Select(x => x.UserId).Distinct().ToList();

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<UserDto> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).BuildDto(users, userFields);

            if (userDtos == null || !userDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
			return shelters.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}

		private async Task<Dictionary<String, List<AnimalDto>>?> CollectAnimals(List<Shelter> shelters, List<String> animalFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> shelterIds = shelters.Select(x => x.Id).Distinct().ToList();

            AnimalLookup animalLookup = new AnimalLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalLookup.PageSize = 1000;
            animalLookup.ShelterIds = shelterIds;
            animalLookup.Fields = animalFields;

            List<Data.Entities.Animal> animals = await animalLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<AnimalDto> animalDtos = await _builderFactory.Builder<AnimalBuilder>().Authorise(this._authorise).BuildDto(animals, animalFields);

            if (animalDtos == null || !animalDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
            Dictionary<String, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> List<AnimalId> ] 
			return animalDtos.GroupBy(a => a.Shelter!.Id).ToDictionary(g => g.Key, g => g.ToList());
		}
	}
}