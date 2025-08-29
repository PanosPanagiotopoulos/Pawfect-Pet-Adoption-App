using AutoMapper;
using Pawfect_API.Data.Entities;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.Models.AdoptionApplication;
using Pawfect_API.Models.Animal;
using Pawfect_API.Models.File;
using Pawfect_API.Models.Lookups;
using Pawfect_API.Models.Shelter;
using Pawfect_API.Models.User;
using Pawfect_API.Query;
using Pawfect_API.Services.FileServices;

namespace Pawfect_API.Builders
{
	public class AutoAdoptionApplicationBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : AdoptionApplication
		public AutoAdoptionApplicationBuilder()
		{
            // Mapping για το Entity : AdoptionApplication σε AdoptionApplication για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.AdoptionApplication, Data.Entities.AdoptionApplication>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.AdoptionApplication, AdoptionApplicationPersist>();
            CreateMap<AdoptionApplicationPersist, Data.Entities.AdoptionApplication>();
		}
	}

	public class AdoptionApplicationBuilder : BaseBuilder<Models.AdoptionApplication.AdoptionApplication, Data.Entities.AdoptionApplication>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        private readonly IFileAccessService _accessService;

        public AdoptionApplicationBuilder(
            IQueryFactory queryFactory,
            IBuilderFactory builderFactory,
			IFileAccessService accessService
        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
            this._accessService = accessService;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
		public AdoptionApplicationBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.AdoptionApplication.AdoptionApplication>> Build(List<Data.Entities.AdoptionApplication> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = base.ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.Animal.Animal>? animalMap = foreignEntitiesFields.ContainsKey(nameof(Models.AdoptionApplication.AdoptionApplication.Animal))
				? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Models.AdoptionApplication.AdoptionApplication.Animal)]))
				: null;

            Dictionary<String, Models.User.User>? userMap = foreignEntitiesFields.ContainsKey(nameof(Models.AdoptionApplication.AdoptionApplication.User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(Models.AdoptionApplication.AdoptionApplication.User)]))
				: null;

            Dictionary<String, Models.Shelter.Shelter>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Models.Shelter.Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Models.AdoptionApplication.AdoptionApplication.Shelter)]))
				: null;

            Dictionary<String, List<Models.File.File>>? filesMap = foreignEntitiesFields.ContainsKey(nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles)]))
				: null;

            List<Models.AdoptionApplication.AdoptionApplication> result = new List<Models.AdoptionApplication.AdoptionApplication>();
			foreach (Data.Entities.AdoptionApplication e in entities)
			{
                Models.AdoptionApplication.AdoptionApplication dto = new Models.AdoptionApplication.AdoptionApplication();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.AdoptionApplication.AdoptionApplication.ApplicationDetails))) dto.ApplicationDetails = e.ApplicationDetails;
				if (nativeFields.Contains(nameof(Models.AdoptionApplication.AdoptionApplication.Status))) dto.Status = e.Status;
				if (nativeFields.Contains(nameof(Models.AdoptionApplication.AdoptionApplication.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.AdoptionApplication.AdoptionApplication.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (animalMap != null && animalMap.ContainsKey(e.Id)) dto.Animal = animalMap[e.Id];
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (filesMap != null && filesMap.ContainsKey(e.Id)) dto.AttachedFiles = filesMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.Animal.Animal>> CollectAnimals(List<Data.Entities.AdoptionApplication> applications, List<String> animalFields)
		{
            if (applications.Count == 0 || animalFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> animalIds = [.. applications.Select(x => x.AnimalId).Distinct()];

			AnimalLookup animalLookup = new AnimalLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalLookup.Offset = 0;
			// Γενική τιμή για τη λήψη των dtos
			animalLookup.PageSize = 1000;
			animalLookup.Ids = animalIds;
			animalLookup.Fields = animalFields;

			List<Data.Entities.Animal> animals = await animalLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.Animal.Animal> animalDtos = await _builderFactory.Builder<AnimalBuilder>().Authorise(this._authorise).Build(animals, animalFields);

			if (animalDtos == null || animalDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
            Dictionary<String, Models.Animal.Animal> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> AnimalId ] 
			return applications.ToDictionary(x => x.Id, x => animalDtoMap[x.AnimalId]);
		}

		private async Task<Dictionary<String, Models.User.User>> CollectUsers(List<Data.Entities.AdoptionApplication> applications, List<String> userFields)
		{
            if (applications.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. applications.Select(x => x.UserId).Distinct()];

			UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 0;
			// Γενική τιμή για τη λήψη των dtos
			userLookup.PageSize = 1000;
			userLookup.Ids = userIds;
			userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

            Dictionary<String, Models.User.User> result = new();

            foreach (Data.Entities.AdoptionApplication app in applications)
            {
                if (userDtoMap.TryGetValue(app.UserId, out Models.User.User userDto))
                {
                    result[app.Id] = userDto;
                }
            }

            return result;
        }

		private async Task<Dictionary<String, Models.Shelter.Shelter>> CollectShelters(List<Data.Entities.AdoptionApplication> applications, List<String> shelterFields)
		{
            if (applications.Count == 0 || shelterFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> shelterIds = [.. applications.Select(x => x.ShelterId).Distinct()];

            ShelterLookup shelterLookup = new ShelterLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            shelterLookup.Offset = 0;
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

			// Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> ShelterId ] 
			return applications.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
		}

		private async Task<Dictionary<String, List<Models.File.File>>> CollectFiles(List<Data.Entities.AdoptionApplication> applications, List<String> fileFields)
		{
            if (applications.Count == 0 || fileFields.Count == 0) return null;

            List<String> fileIds = [.. applications
				.Where(x => x.AttachedFilesIds != null)
				.SelectMany(x => x.AttachedFilesIds)
				.Distinct()];

			FileLookup fileLookup = new FileLookup();

			fileLookup.Offset = 0;
			fileLookup.PageSize = 1000;
			fileLookup.Ids = fileIds;
			fileLookup.Fields = fileFields;

            List<Data.Entities.File> files = await fileLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

			await _accessService.AttachUrlsAsync(files);

            // Κατασκευή των dtos
            List<Models.File.File> fileDtos = await _builderFactory.Builder<FileBuilder>().Authorise(this._authorise).Build(files, fileFields);

            if (fileDtos == null || fileDtos.Count == 0) return null;

            Dictionary<String, Models.File.File> fileDtoMap = fileDtos.ToDictionary(x => x.Id);

			return applications.ToDictionary(
				app => app.Id,
				app => app.AttachedFilesIds?.Select(fileId => fileDtoMap.TryGetValue(fileId, out Models.File.File fileDto) ? fileDto : null)
					.Where(fileDto => fileDto != null)
					.ToList() ?? null
			);
		}
	}
}