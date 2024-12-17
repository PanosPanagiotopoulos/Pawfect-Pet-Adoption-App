using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services;

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
        private readonly IAnimalService _animalService;
        private readonly UserLookup _userLookup;
        private readonly IUserService _userService;
        private readonly ShelterLookup _shelterLookup;
        private readonly IShelterService _shelterService;

        public AdoptionApplicationBuilder(AnimalLookup animalLookup, IAnimalService animalService
                                         , UserLookup userLookup, IUserService userService
                                         , ShelterLookup shelterLookup, IShelterService shelterService)
        {
            _animalLookup = animalLookup;
            _animalService = animalService;
            _userLookup = userLookup;
            _userService = userService;
            _shelterLookup = shelterLookup;
            _shelterService = shelterService;
        }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<AdoptionApplicationDto>> BuildDto(List<AdoptionApplication> entities, List<string> fields)
        {
            // Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
            (List<string> nativeFields, Dictionary<string, List<string>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο string ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<string, AnimalDto>? animalMap = foreignEntitiesFields.ContainsKey(nameof(Animal))
                ? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Animal)]))
                : null;

            Dictionary<string, UserDto>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
                ? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
                : null;

            Dictionary<string, ShelterDto>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Shelter))
                ? (await CollectShelters(entities, foreignEntitiesFields[nameof(Shelter)]))
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

                result.Add(dto);
            }

            return await Task.FromResult(result);
        }

        private async Task<Dictionary<string, AnimalDto>> CollectAnimals(List<AdoptionApplication> applications, List<string> animalFields)
        {
            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<string> animalIds = applications.Select(x => x.AnimalId).Distinct().ToList();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            _animalLookup.Offset = LookupParams.Offset;
            // Γενική τιμή για τη λήψη των dtos
            _animalLookup.PageSize = LookupParams.PageSize;
            _animalLookup.SortDescending = LookupParams.SortDescending;
            _animalLookup.Query = null;
            _animalLookup.Ids = animalIds;
            _animalLookup.Fields = animalFields;

            // Κατασκευή των dtos
            List<AnimalDto> animalDtos = (await _animalService.QueryAnimalsAsync(_animalLookup)).ToList();

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
            Dictionary<string, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> AnimalId ] 
            return applications.ToDictionary(x => x.Id, x => animalDtoMap[x.AnimalId]);
        }

        private async Task<Dictionary<string, UserDto>> CollectUsers(List<AdoptionApplication> applications, List<string> userFields)
        {
            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<string> userIds = applications.Select(x => x.UserId).Distinct().ToList();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            _animalLookup.Offset = LookupParams.Offset;
            // Γενική τιμή για τη λήψη των dtos
            _animalLookup.PageSize = LookupParams.PageSize;
            _animalLookup.SortDescending = LookupParams.SortDescending;
            _animalLookup.Query = null;
            _animalLookup.Ids = userIds;
            _animalLookup.Fields = userFields;

            // Κατασκευή των dtos
            List<UserDto> userDtos = (await _userService.QueryUsersAsync(_userLookup)).ToList();

            // Δημιουργία ενός Dictionary με τον τύπο string ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<string, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> UserId ] 
            return applications.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
        }

        private async Task<Dictionary<string, ShelterDto>> CollectShelters(List<AdoptionApplication> applications, List<string> shelterFields)
        {
            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<string> shelterIds = applications.Select(x => x.ShelterId).Distinct().ToList();

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

            // Δημιουργία ενός Dictionary με τον τύπο string ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
            Dictionary<string, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα applications δημιουργώντας ένα Dictionary : [ ApplicationId -> ShelterId ] 
            return applications.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
        }
    }
}