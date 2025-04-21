using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;
using Pawfect_Pet_Adoption_App_API.Services.FileServices;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoAdoptionApplicationBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : AdoptionApplication
		public AutoAdoptionApplicationBuilder()
		{
			// Mapping για το Entity : AdoptionApplication σε AdoptionApplication για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<AdoptionApplication, AdoptionApplication>();

			// POST Request Dto Μοντέλα
			CreateMap<AdoptionApplication, AdoptionApplicationPersist>();
			CreateMap<AdoptionApplicationPersist, AdoptionApplication>();
		}
	}

	public class AdoptionApplicationBuilder : BaseBuilder<AdoptionApplicationDto, AdoptionApplication>
	{
		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<AdoptionApplicationDto, AdoptionApplication> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

		private readonly AnimalLookup _animalLookup;
		private readonly Lazy<IAnimalService> _animalService;
		private readonly UserLookup _userLookup;
		private readonly Lazy<IUserService> _userService;
		private readonly ShelterLookup _shelterLookup;
		private readonly IShelterService _shelterService;
		private readonly FileLookup _fileLookup;
		private readonly IFileService _fileService;

		public AdoptionApplicationBuilder(
			AnimalLookup animalLookup, Lazy<IAnimalService> animalService, 
			UserLookup userLookup, Lazy<IUserService> userService,
			ShelterLookup shelterLookup, IShelterService shelterService,
			FileLookup fileLookup, IFileService fileService

			)
		{
			_animalLookup = animalLookup;
			_animalService = animalService;
			_userLookup = userLookup;
			_userService = userService;
			_shelterLookup = shelterLookup;
			_shelterService = shelterService;
			_fileLookup = fileLookup;
			_fileService = fileService;
		}

		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<AdoptionApplicationDto>> BuildDto(List<AdoptionApplication> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, AnimalDto>? animalMap = foreignEntitiesFields.ContainsKey(nameof(Animal))
				? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Animal)]))
				: null;

			Dictionary<String, UserDto>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			Dictionary<String, ShelterDto>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Shelter)]))
				: null;

			Dictionary<String, List<FileDto>>? filesMap = foreignEntitiesFields.ContainsKey(nameof(Data.Entities.File))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Data.Entities.File)]))
				: null;

			List<AdoptionApplicationDto> result = new List<AdoptionApplicationDto>();
			foreach (AdoptionApplication e in entities)
			{
				AdoptionApplicationDto dto = new AdoptionApplicationDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(AdoptionApplication.ApplicationDetails))) dto.ApplicationDetails = e.ApplicationDetails;
				if (nativeFields.Contains(nameof(AdoptionApplication.Status))) dto.Status = e.Status;
				if (nativeFields.Contains(nameof(AdoptionApplication.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(AdoptionApplication.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (animalMap != null && animalMap.ContainsKey(e.Id)) dto.Animal = animalMap[e.Id];
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (filesMap != null && filesMap.ContainsKey(e.Id)) dto.AttachedFiles = filesMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, AnimalDto>> CollectAnimals(List<AdoptionApplication> applications, List<String> animalFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalIds = applications.Select(x => x.AnimalId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_animalLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_animalLookup.PageSize = LookupParams.PageSize;
			_animalLookup.SortDescending = LookupParams.SortDescending;
			_animalLookup.Query = null;
			_animalLookup.Ids = animalIds;
			_animalLookup.Fields = animalFields;

			// Κατασκευή των dtos
			List<AnimalDto> animalDtos = (await _animalService.Value.QueryAnimalsAsync(_animalLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
			Dictionary<String, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> AnimalId ] 
			return applications.ToDictionary(x => x.Id, x => animalDtoMap[x.AnimalId]);
		}

		private async Task<Dictionary<String, UserDto>> CollectUsers(List<AdoptionApplication> applications, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = applications.Select(x => x.UserId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_animalLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_animalLookup.PageSize = LookupParams.PageSize;
			_animalLookup.SortDescending = LookupParams.SortDescending;
			_animalLookup.Query = null;
			_animalLookup.Ids = userIds;
			_animalLookup.Fields = userFields;

			// Κατασκευή των dtos
			List<UserDto> userDtos = (await _userService.Value.QueryUsersAsync(_userLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
			Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> UserId ] 
			return applications.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}

		private async Task<Dictionary<String, ShelterDto>> CollectShelters(List<AdoptionApplication> applications, List<String> shelterFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> shelterIds = applications.Select(x => x.ShelterId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_animalLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_animalLookup.PageSize = LookupParams.PageSize;
			_animalLookup.SortDescending = LookupParams.SortDescending;
			_animalLookup.Query = null;
			_animalLookup.Ids = shelterIds;
			_animalLookup.Fields = shelterFields;

			// Κατασκευή των dtos
			List<ShelterDto> shelterDtos = (await _shelterService.QuerySheltersAsync(_shelterLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
			Dictionary<String, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> ShelterId ] 
			return applications.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
		}

		private async Task<Dictionary<String, List<FileDto>>> CollectFiles(List<AdoptionApplication> adoptionApplications, List<String> fileFields)
		{
			List<String> fileIds = adoptionApplications
				.Where(x => x.AttachedFilesIds != null)
				.SelectMany(x => x.AttachedFilesIds)
				.Distinct()
				.ToList();

			_fileLookup.Offset = LookupParams.Offset;
			_fileLookup.PageSize = LookupParams.PageSize;
			_fileLookup.SortDescending = LookupParams.SortDescending;
			_fileLookup.Query = null;
			_fileLookup.Ids = fileIds;
			_fileLookup.Fields = fileFields;

			List<FileDto> fileDtos = (await _fileService.QueryFilesAsync(_fileLookup)).ToList();

			if (fileDtos == null || !fileDtos.Any()) return null;

			Dictionary<String, FileDto> fileDtoMap = fileDtos.ToDictionary(x => x.Id);

			return adoptionApplications.ToDictionary(
				app => app.Id,
				app => app.AttachedFilesIds?.Select(fileId => fileDtoMap.TryGetValue(fileId, out FileDto fileDto) ? fileDto : null)
					.Where(fileDto => fileDto != null)
					.ToList() ?? null
			);
		}
	}
}