using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices;
using Pawfect_Pet_Adoption_App_API.Services.BreedServices;
using Pawfect_Pet_Adoption_App_API.Services.ShelterServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoAnimalBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Animal
		public AutoAnimalBuilder()
		{
			// Mapping για το Entity : Animal σε Animal για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Animal, Animal>();

			// POST Request Dto Μοντέλα
			CreateMap<Animal, AnimalPersist>();
			CreateMap<AnimalPersist, Animal>();
		}

	}

	public class AnimalBuilder : BaseBuilder<AnimalDto, Animal>
	{
		private readonly BreedLookup _breedLookup;
		private readonly Lazy<IBreedService> _breedService;
		private readonly ShelterLookup _shelterLookup;
		private readonly Lazy<IShelterService> _shelterService;
		private readonly AnimalTypeLookup _animalTypeLookup;
		private readonly Lazy<IAnimalTypeService> _animalTypeService;

		public AnimalBuilder(BreedLookup breedLookup, Lazy<IBreedService> breedService,
							 ShelterLookup shelterLookup, Lazy<IShelterService> shelterService
							, AnimalTypeLookup animalTypeLookup, Lazy<IAnimalTypeService> animalTypeService)
		{
			_breedLookup = breedLookup;
			_breedService = breedService;
			_shelterLookup = shelterLookup;
			_shelterService = shelterService;
			_animalTypeLookup = animalTypeLookup;
			_animalTypeService = animalTypeService;
		}


		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<AnimalDto, Animal> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<AnimalDto>> BuildDto(List<Animal> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, BreedDto>? breedMap = foreignEntitiesFields.ContainsKey(nameof(Breed))
				? (await CollectBreeds(entities, foreignEntitiesFields[nameof(Breed)]))
				: null;

			Dictionary<String, ShelterDto>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Shelter)]))
				: null;


			Dictionary<String, AnimalTypeDto>? animalTypeMap = foreignEntitiesFields.ContainsKey(nameof(AnimalType))
				? (await CollectAnimalTypes(entities, foreignEntitiesFields[nameof(AnimalType)]))
				: null;


			List<AnimalDto> result = new List<AnimalDto>();
			foreach (Animal e in entities)
			{
				AnimalDto dto = new AnimalDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Animal.Name))) dto.Name = e.Name;
				if (nativeFields.Contains(nameof(Animal.Age))) dto.Age = e.Age;
				if (nativeFields.Contains(nameof(Animal.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Animal.Gender))) dto.Gender = e.Gender;
				if (nativeFields.Contains(nameof(Animal.Weight))) dto.Weight = e.Weight;
				if (nativeFields.Contains(nameof(Animal.HealthStatus))) dto.HealthStatus = e.HealthStatus;
				if (nativeFields.Contains(nameof(Animal.Photos))) dto.Photos = e.Photos;
				if (nativeFields.Contains(nameof(Animal.AdoptionStatus))) dto.AdoptionStatus = e.AdoptionStatus;
				if (nativeFields.Contains(nameof(Animal.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Animal.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (breedMap != null && breedMap.ContainsKey(e.Id)) dto.Breed = breedMap[e.Id];
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (animalTypeMap != null && animalTypeMap.ContainsKey(e.Id)) dto.Type = animalTypeMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, BreedDto>> CollectBreeds(List<Animal> animals, List<String> breedFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> breedIds = animals.Select(x => x.BreedId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_breedLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_breedLookup.PageSize = LookupParams.PageSize;
			_breedLookup.SortDescending = LookupParams.SortDescending;
			_breedLookup.Query = null;
			_breedLookup.Ids = breedIds;
			_breedLookup.Fields = breedFields;

			// Κατασκευή των dtos
			List<BreedDto> breedDtos = (await _breedService.Value.QueryBreedsAsync(_breedLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ BreedId -> BreedDto ]
			Dictionary<String, BreedDto> breedDtoMap = breedDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> BreedId ] 
			return animals.ToDictionary(x => x.Id, x => breedDtoMap[x.BreedId]);
		}

		private async Task<Dictionary<String, AnimalTypeDto>> CollectAnimalTypes(List<Animal> animals, List<String> animalTypesFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalTypeIds = animals.Select(x => x.TypeId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_breedLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_breedLookup.PageSize = LookupParams.PageSize;
			_breedLookup.SortDescending = LookupParams.SortDescending;
			_breedLookup.Query = null;
			_breedLookup.Ids = animalTypeIds;
			_breedLookup.Fields = animalTypesFields;

			// Κατασκευή των dtos
			List<AnimalTypeDto> animalTypeDtos = (await _animalTypeService.Value.QueryAnimalTypesAsync(_animalTypeLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalTypeId -> AnimalTypeDto ]
			Dictionary<String, AnimalTypeDto> animalTypeDtoMap = animalTypeDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> AnimalTypeId ] 
			return animals.ToDictionary(x => x.Id, x => animalTypeDtoMap[x.TypeId]);
		}

		private async Task<Dictionary<String, ShelterDto>> CollectShelters(List<Animal> animals, List<String> shelterFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> shelterIds = animals.Select(x => x.ShelterId).Distinct().ToList();

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_breedLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_breedLookup.PageSize = LookupParams.PageSize;
			_breedLookup.SortDescending = LookupParams.SortDescending;
			_breedLookup.Query = null;
			_breedLookup.Ids = shelterIds;
			_breedLookup.Fields = shelterFields;

			// Κατασκευή των dtos
			List<ShelterDto> shelterDtos = (await _shelterService.Value.QuerySheltersAsync(_shelterLookup)).ToList();

			// Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
			Dictionary<String, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> ShelterId ] 
			return animals.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
		}
	}
}