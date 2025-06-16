using AutoMapper;

using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.Lookups;
using Main_API.Models.User;
using Main_API.Query;
namespace Main_API.Builders
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
            CreateMap<Data.Entities.User, Data.Entities.User>();
			CreateMap<Models.User.User, Data.Entities.User>().ReverseMap();


            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.User, UserPersist>();
            CreateMap<UserPersist, Data.Entities.User>();
		}
	}
	public class UserBuilder : BaseBuilder<Models.User.User, Data.Entities.User>
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
        public override async Task<List<Models.User.User>> Build(List<Data.Entities.User> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = this.ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.Shelter.Shelter?>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Models.User.User.Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Models.User.User.Shelter)]))
				: null;

            Dictionary<String, Models.File.File>? fileMap = foreignEntitiesFields.ContainsKey(nameof(Models.User.User.ProfilePhoto))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Models.User.User.ProfilePhoto)]))
				: null;

            List<Models.User.User> result = new List<Models.User.User>();
			foreach (Data.Entities.User e in entities)
			{
                Models.User.User dto = new Models.User.User();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.User.User.Email))) dto.Email = e.Email;
				if (nativeFields.Contains(nameof(Models.User.User.FullName))) dto.FullName = e.FullName;
				if (nativeFields.Contains(nameof(Models.User.User.Roles))) dto.Roles = e.Roles;
				if (nativeFields.Contains(nameof(Models.User.User.Phone))) dto.Phone = e.Phone;
				if (nativeFields.Contains(nameof(Models.User.User.Location))) dto.Location = e.Location;
				if (nativeFields.Contains(nameof(Models.User.User.AuthProvider))) dto.AuthProvider = e.AuthProvider;
				if (nativeFields.Contains(nameof(Models.User.User.AuthProviderId))) dto.AuthProviderId = e.AuthProviderId;
				if (nativeFields.Contains(nameof(Models.User.User.IsVerified))) dto.IsVerified = e.IsVerified;
				if (nativeFields.Contains(nameof(Models.User.User.HasPhoneVerified))) dto.HasPhoneVerified = e.HasPhoneVerified;
				if (nativeFields.Contains(nameof(Models.User.User.HasEmailVerified))) dto.HasEmailVerified = e.HasEmailVerified;
				if (nativeFields.Contains(nameof(Models.User.User.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.User.User.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (fileMap != null && fileMap.ContainsKey(e.Id)) dto.ProfilePhoto = fileMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.Shelter.Shelter?>?> CollectShelters(List<Data.Entities.User> users, List<String> shelterFields)
		{
            if (users.Count == 0 || shelterFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String?> shelterIds = [.. users.Where(x => !String.IsNullOrEmpty(x.ShelterId)).Select(x => x.ShelterId).Distinct()];

            ShelterLookup shelterLookup = new ShelterLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            shelterLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            shelterLookup.PageSize = 1000;
            shelterLookup.Ids = shelterIds;
            shelterLookup.Fields = shelterFields;

            List<Data.Entities.Shelter> shelters = await shelterLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.Shelter.Shelter> shelterDtos = await _builderFactory.Builder<ShelterBuilder>().Authorise(this._authorise).Build(shelters, shelterFields);

            if (shelterDtos == null || shelterDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
            Dictionary<String, Models.Shelter.Shelter> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τους users δημιουργώντας ένα Dictionary : [ UserId -> ShelterId ] 
			return users.ToDictionary(x => x.Id, x => !String.IsNullOrEmpty(x.ShelterId) ? shelterDtoMap[x.ShelterId] : null);
		}

		private async Task<Dictionary<String, Models.File.File>> CollectFiles(List<Data.Entities.User> users, List<String> fileFields)
		{
            if (users.Count == 0 || fileFields.Count == 0) return null;

            List<String> fileIds = [.. users
				.Where(x => !String.IsNullOrEmpty(x.ProfilePhotoId))
				.Select(x => x.ProfilePhotoId)
				.Distinct()];

			if (fileIds.Count == 0) return null;

            FileLookup fileLookup = new FileLookup();

            fileLookup.Offset = 1;
            fileLookup.PageSize = 1000;
            fileLookup.Ids = fileIds;
            fileLookup.Fields = fileFields;

            List<Data.Entities.File> files = await fileLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.File.File> fileDtos = await _builderFactory.Builder<FileBuilder>().Authorise(this._authorise).Build(files, fileFields);

            if (fileDtos == null || fileDtos.Count == 0) return null;

            Dictionary<String, Models.File.File> fileDtoMap = fileDtos.ToDictionary(x => x.Id);

			return users.ToDictionary(x => x.Id, x => fileDtoMap.GetValueOrDefault(x.ProfilePhotoId ?? ""));
		}
	}
}
