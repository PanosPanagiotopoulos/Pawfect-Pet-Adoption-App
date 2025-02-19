using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoUserBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : User
		public AutoUserBuilder()
		{
			// Mapping για nested object : Location
			CreateMap<Location, Location>();

			// Mapping για το Entity : User σε User για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<User, User>();

			// POST Request Dto Μοντέλα
			CreateMap<User, UserPersist>();
			CreateMap<UserPersist, User>();
		}
	}

	public class UserBuilder : BaseBuilder<UserDto, User>
	{
		private readonly ShelterLookup _shelterLookup;
		private readonly Lazy<IShelterService> _shelterService;

		public UserBuilder(ShelterLookup shelterLookup, Lazy<IShelterService> shelterService)
		{
			_shelterLookup = shelterLookup;
			_shelterService = shelterService;
		}

		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<UserDto, User> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<UserDto>> BuildDto(List<User> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, ShelterDto?>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Shelter)]))
				: null;

			List<UserDto> result = new List<UserDto>();
			foreach (User e in entities)
			{
				UserDto dto = new UserDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(User.Email))) dto.Email = e.Email;
				if (nativeFields.Contains(nameof(User.FullName))) dto.FullName = e.FullName;
				if (nativeFields.Contains(nameof(User.Role))) dto.Role = e.Role;
				if (nativeFields.Contains(nameof(User.Phone))) dto.Phone = e.Phone;
				if (nativeFields.Contains(nameof(User.Location))) dto.Location = e.Location;
				if (nativeFields.Contains(nameof(User.AuthProvider))) dto.AuthProvider = e.AuthProvider;
				if (nativeFields.Contains(nameof(User.IsVerified))) dto.IsVerified = e.IsVerified;
				if (nativeFields.Contains(nameof(User.HasPhoneVerified))) dto.HasPhoneVerified = e.HasPhoneVerified;
				if (nativeFields.Contains(nameof(User.HasEmailVerified))) dto.HasEmailVerified = e.HasEmailVerified;
				if (nativeFields.Contains(nameof(User.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(User.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, ShelterDto?>?> CollectShelters(List<User> users, List<String> shelterFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String?> shelterIds = users.Where(x => !String.IsNullOrEmpty(x.ShelterId)).Select(x => x.ShelterId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_shelterLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_shelterLookup.PageSize = LookupParams.PageSize;
			_shelterLookup.SortDescending = LookupParams.SortDescending;
			_shelterLookup.Query = null;
			_shelterLookup.Ids = shelterIds;
			_shelterLookup.Fields = shelterFields;

			// Κατασκευή των dtos
			List<ShelterDto> shelterDtos = (await _shelterService.Value.QuerySheltersAsync(_shelterLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
			Dictionary<String, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			if (shelterDtoMap.Count == 0) { return null; }

			// Ταίριασμα του προηγούμενου Dictionary με τους users δημιουργώντας ένα Dictionary : [ UserId -> ShelterId ] 
			return users.ToDictionary(x => x.Id, x => !String.IsNullOrEmpty(x.ShelterId) ? shelterDtoMap[x.ShelterId] : null);
		}
	}
}
