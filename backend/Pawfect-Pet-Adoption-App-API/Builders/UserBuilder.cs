using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;
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
			CreateMap<User, UserDto>().ReverseMap();


			// POST Request Dto Μοντέλα
			CreateMap<User, UserPersist>();
			CreateMap<UserPersist, User>();
		}
	}

	public class UserBuilder : BaseBuilder<UserDto, User>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public UserBuilder
		(
			IQueryFactory queryFactory, IBuilderFactory builderFactory

        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public UserBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<UserDto>> BuildDto(List<User> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, ShelterDto?>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Shelter)]))
				: null;

			Dictionary<String, FileDto>? fileMap = foreignEntitiesFields.ContainsKey(nameof(Data.Entities.File))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Data.Entities.File)]))
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
				if (nativeFields.Contains(nameof(User.AuthProviderId))) dto.AuthProviderId = e.AuthProviderId;
				if (nativeFields.Contains(nameof(User.IsVerified))) dto.IsVerified = e.IsVerified;
				if (nativeFields.Contains(nameof(User.HasPhoneVerified))) dto.HasPhoneVerified = e.HasPhoneVerified;
				if (nativeFields.Contains(nameof(User.HasEmailVerified))) dto.HasEmailVerified = e.HasEmailVerified;
				if (nativeFields.Contains(nameof(User.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(User.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (fileMap != null && fileMap.ContainsKey(e.Id)) dto.ProfilePhoto = fileMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, ShelterDto?>?> CollectShelters(List<User> users, List<String> shelterFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String?> shelterIds = users.Where(x => !String.IsNullOrEmpty(x.ShelterId)).Select(x => x.ShelterId).Distinct().ToList();

            ShelterLookup shelterLookup = new ShelterLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            shelterLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            shelterLookup.PageSize = 1000;
            shelterLookup.Ids = shelterIds;
            shelterLookup.Fields = shelterFields;

            List<Data.Entities.Shelter> shelters = await shelterLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<ShelterDto> shelterDtos = await _builderFactory.Builder<ShelterBuilder>().Authorise(this._authorise).BuildDto(shelters, shelterFields);

            if (shelterDtos == null || !shelterDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
            Dictionary<String, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τους users δημιουργώντας ένα Dictionary : [ UserId -> ShelterId ] 
			return users.ToDictionary(x => x.Id, x => !String.IsNullOrEmpty(x.ShelterId) ? shelterDtoMap[x.ShelterId] : null);
		}

		private async Task<Dictionary<String, FileDto>> CollectFiles(List<User> users, List<String> fileFields)
		{
			List<String> fileIds = users
				.Where(x => x.ProfilePhotoId != null)
				.Select(x => x.ProfilePhotoId)
				.Distinct()
				.ToList();

            FileLookup fileLookup = new FileLookup();

            fileLookup.Offset = 1;
            fileLookup.PageSize = 1000;
            fileLookup.Ids = fileIds;
            fileLookup.Fields = fileFields;

            List<Data.Entities.File> files = await fileLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<FileDto> fileDtos = await _builderFactory.Builder<FileBuilder>().Authorise(this._authorise).BuildDto(files, fileFields);

            if (fileDtos == null || !fileDtos.Any()) return null;

			Dictionary<String, FileDto> fileDtoMap = fileDtos.ToDictionary(x => x.Id);

			return users.ToDictionary(x => x.Id, x => fileDtoMap[x.ProfilePhotoId]);
		}
	}
}
