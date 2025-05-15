using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Query;

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
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public AnimalBuilder
		(
			IQueryFactory queryFactory,
			IBuilderFactory builderFactory	
		)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }


        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        public AnimalBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

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

			Dictionary<String, List<FileDto>>? filesMap = foreignEntitiesFields.ContainsKey(nameof(Data.Entities.File))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Data.Entities.File)]))
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
				if (nativeFields.Contains(nameof(Animal.AdoptionStatus))) dto.AdoptionStatus = e.AdoptionStatus;
				if (nativeFields.Contains(nameof(Animal.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Animal.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (breedMap != null && breedMap.ContainsKey(e.Id)) dto.Breed = breedMap[e.Id];
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (animalTypeMap != null && animalTypeMap.ContainsKey(e.Id)) dto.AnimalType = animalTypeMap[e.Id];
				if (filesMap != null && filesMap.ContainsKey(e.Id)) dto.Photos = filesMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, BreedDto>> CollectBreeds(List<Animal> animals, List<String> breedFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> breedIds = animals.Select(x => x.BreedId).Distinct().ToList();


            BreedLookup breedLookup = new BreedLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            breedLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            breedLookup.PageSize = 1000;
            breedLookup.Ids = breedIds;
            breedLookup.Fields = breedFields;

			List<Data.Entities.Breed> breeds = await breedLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

			List<BreedDto> breedDtos = await _builderFactory.Builder<BreedBuilder>().Authorise(this._authorise).BuildDto(breeds, breedFields);

            if (breedDtos == null || !breedDtos.Any()) { return null; }

			// Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ BreedId -> BreedDto ]
			Dictionary<String, BreedDto> breedDtoMap = breedDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> BreedId ] 
			return animals.ToDictionary(x => x.Id, x => breedDtoMap[x.BreedId]);
		}

		private async Task<Dictionary<String, AnimalTypeDto>> CollectAnimalTypes(List<Animal> animals, List<String> animalTypesFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalTypeIds = animals.Select(x => x.AnimalTypeId).Distinct().ToList();

			AnimalTypeLookup animalTypeLookup = new AnimalTypeLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalTypeLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalTypeLookup.PageSize = 1000;
            animalTypeLookup.Ids = animalTypeIds;
            animalTypeLookup.Fields = animalTypesFields;

            List<Data.Entities.AnimalType> animalTypes = await animalTypeLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<AnimalTypeDto> animalTypeDtos = await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(this._authorise).BuildDto(animalTypes, animalTypesFields);

            if (animalTypeDtos == null || !animalTypeDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalTypeId -> AnimalTypeDto ]
            Dictionary<String, AnimalTypeDto> animalTypeDtoMap = animalTypeDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> AnimalTypeId ] 
			return animals.ToDictionary(x => x.Id, x => animalTypeDtoMap[x.AnimalTypeId]);
		}

		private async Task<Dictionary<String, ShelterDto>> CollectShelters(List<Animal> animals, List<String> shelterFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> shelterIds = animals.Select(x => x.ShelterId).Distinct().ToList();

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

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
            Dictionary<String, ShelterDto> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> ShelterId ] 
			return animals.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
		}

		private async Task<Dictionary<String, List<FileDto>>> CollectFiles(List<Animal> animals, List<String> fileFields)
		{
			List<String> fileIds = animals
				.Where(x => x.PhotosIds != null)
				.SelectMany(x => x.PhotosIds)
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

			return animals.ToDictionary(
				animal => animal.Id,
				app => app.PhotosIds?.Select(fileId => fileDtoMap.TryGetValue(fileId, out FileDto fileDto) ? fileDto : null)
					.Where(fileDto => fileDto != null)
					.ToList() ?? null
			);
		}
	}
}