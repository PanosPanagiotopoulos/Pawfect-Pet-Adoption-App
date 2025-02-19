using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
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

		public ShelterBuilder(UserLookup userLookup, Lazy<IUserService> userService)
		{
			_userLookup = userLookup;
			_userService = userService;
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

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, UserDto>> CollectUsers(List<Shelter> shelters, List<String> userFields)
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

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
			Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
			return shelters.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}
	}
}