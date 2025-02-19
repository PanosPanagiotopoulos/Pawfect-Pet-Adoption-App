using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
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
		private readonly UserLookup _userLookup;
		private readonly Lazy<IUserService> _userService;
		private readonly AnimalLookup _animalLookup;
		private readonly Lazy<IAnimalService> _animalService;

		public ShelterBuilder
		(
		  UserLookup userLookup,
		  Lazy<IUserService> userService,
		  AnimalLookup animalLookup,
		  Lazy<IAnimalService> animalService
		)
		{
			_userLookup = userLookup;
			_userService = userService;
			_animalLookup = animalLookup;
			_animalService = animalService;
		}

		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<ShelterDto, Shelter> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

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

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_userLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_userLookup.PageSize = LookupParams.PageSize;
			_userLookup.SortDescending = LookupParams.SortDescending;
			_userLookup.Query = null;
			_userLookup.Ids = userIds;
			_userLookup.Fields = userFields;

			// Κατασκευή των dtos
			List<UserDto> userDtos = (await _userService.Value.QueryUsersAsync(_userLookup)).ToList();

			if (userDtos == null || !userDtos.Any()) return null;

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
			Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
			return shelters.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}

		private async Task<Dictionary<String, List<AnimalDto>>?> CollectAnimals(List<Shelter> shelters, List<String> animalFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> shelterIds = shelters.Select(x => x.Id).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_animalLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_animalLookup.PageSize = LookupParams.PageSize;
			_animalLookup.SortDescending = LookupParams.SortDescending;
			_animalLookup.Query = null;
			_animalLookup.ShelterIds = shelterIds;
			// Ensure ShelterId will come back from the build
			if (!animalFields.Contains(nameof(Shelter) + "." + nameof(Shelter.Id))) animalFields.Add(nameof(Shelter) + "." + nameof(Shelter.Id));
			_animalLookup.Fields = animalFields;

			// Κατασκευή των dtos
			List<AnimalDto> animalDtos = (await _animalService.Value.QueryAnimalsAsync(_animalLookup)).ToList();

			if (animalDtos == null || !animalDtos.Any()) return null;

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
			Dictionary<String, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> List<AnimalId> ] 
			return animalDtos.GroupBy(a => a.Shelter!.Id).ToDictionary(g => g.Key, g => g.ToList());
		}
	}
}