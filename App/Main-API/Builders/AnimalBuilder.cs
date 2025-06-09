using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
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
            CreateMap<Data.Entities.Animal, Data.Entities.Animal>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Animal, AnimalPersist>();
            CreateMap<AnimalPersist, Data.Entities.Animal>();
		}

	}

	public class AnimalBuilder : BaseBuilder<Models.Animal.Animal, Data.Entities.Animal>
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
        public override async Task<List<Models.Animal.Animal>> Build(List<Data.Entities.Animal> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.Breed.Breed>? breedMap = foreignEntitiesFields.ContainsKey(nameof(Models.Animal.Animal.Breed))
				? (await CollectBreeds(entities, foreignEntitiesFields[nameof(Models.Animal.Animal.Breed)]))
				: null;

            Dictionary<String, Models.Shelter.Shelter>? shelterMap = foreignEntitiesFields.ContainsKey(nameof(Models.Animal.Animal.Shelter))
				? (await CollectShelters(entities, foreignEntitiesFields[nameof(Models.Animal.Animal.Shelter)]))
				: null;


            Dictionary<String, Models.AnimalType.AnimalType>? animalTypeMap = foreignEntitiesFields.ContainsKey(nameof(Models.Animal.Animal.AnimalType))
				? (await CollectAnimalTypes(entities, foreignEntitiesFields[nameof(Models.Animal.Animal.AnimalType)]))
				: null;

            Dictionary<String, List<Models.File.File>>? filesMap = foreignEntitiesFields.ContainsKey(nameof(Models.Animal.Animal.AttachedPhotos))
				? (await CollectFiles(entities, foreignEntitiesFields[nameof(Models.Animal.Animal.AttachedPhotos)]))
				: null;


            List<Models.Animal.Animal> result = new List<Models.Animal.Animal>();
			foreach (Data.Entities.Animal e in entities)
			{
                Models.Animal.Animal dto = new Models.Animal.Animal();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.Name))) dto.Name = e.Name;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.Age))) dto.Age = e.Age;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.Gender))) dto.Gender = e.Gender;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.Weight))) dto.Weight = e.Weight;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.HealthStatus))) dto.HealthStatus = e.HealthStatus;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.AdoptionStatus))) dto.AdoptionStatus = e.AdoptionStatus;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.Animal.Animal.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (breedMap != null && breedMap.ContainsKey(e.Id)) dto.Breed = breedMap[e.Id];
				if (shelterMap != null && shelterMap.ContainsKey(e.Id)) dto.Shelter = shelterMap[e.Id];
				if (animalTypeMap != null && animalTypeMap.ContainsKey(e.Id)) dto.AnimalType = animalTypeMap[e.Id];
				if (filesMap != null && filesMap.ContainsKey(e.Id)) dto.AttachedPhotos = filesMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.Breed.Breed>> CollectBreeds(List<Data.Entities.Animal> animals, List<String> breedFields)
		{
			if (animals.Count == 0 || breedFields.Count == 0) return null;

			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> breedIds = [.. animals.Select(x => x.BreedId).Distinct()];


            BreedLookup breedLookup = new BreedLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            breedLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            breedLookup.PageSize = 1000;
            breedLookup.Ids = breedIds;
            breedLookup.Fields = breedFields;

			List<Data.Entities.Breed> breeds = await breedLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.Breed.Breed> breedDtos = await _builderFactory.Builder<BreedBuilder>().Authorise(this._authorise).Build(breeds, breedFields);

            if (breedDtos == null || breedDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ BreedId -> BreedDto ]
            Dictionary<String, Models.Breed.Breed> breedDtoMap = breedDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> BreedId ] 
			return animals.ToDictionary(x => x.Id, x => breedDtoMap[x.BreedId]);
		}

		private async Task<Dictionary<String, Models.AnimalType.AnimalType>> CollectAnimalTypes(List<Data.Entities.Animal> animals, List<String> animalTypesFields)
		{
            if (animals.Count == 0 || animalTypesFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> animalTypeIds = [.. animals.Select(x => x.AnimalTypeId).Distinct()];

			AnimalTypeLookup animalTypeLookup = new AnimalTypeLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalTypeLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalTypeLookup.PageSize = 1000;
            animalTypeLookup.Ids = animalTypeIds;
            animalTypeLookup.Fields = animalTypesFields;

            List<Data.Entities.AnimalType> animalTypes = await animalTypeLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.AnimalType.AnimalType> animalTypeDtos = await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(this._authorise).Build(animalTypes, animalTypesFields);

            if (animalTypeDtos == null || animalTypeDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AnimalTypeId -> AnimalTypeDto ]
            Dictionary<String, Models.AnimalType.AnimalType> animalTypeDtoMap = animalTypeDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> AnimalTypeId ] 
			return animals.ToDictionary(x => x.Id, x => animalTypeDtoMap.GetValueOrDefault(x.AnimalTypeId ?? ""));
		}

		private async Task<Dictionary<String, Models.Shelter.Shelter>> CollectShelters(List<Data.Entities.Animal> animals, List<String> shelterFields)
		{
            if (animals.Count == 0 || shelterFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> shelterIds = [.. animals.Select(x => x.ShelterId).Distinct()];

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

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ ShelterId -> ShelterDto ]
            Dictionary<String, Models.Shelter.Shelter> shelterDtoMap = shelterDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα animals δημιουργώντας ένα Dictionary : [ AnimalId -> ShelterId ] 
			return animals.ToDictionary(x => x.Id, x => shelterDtoMap[x.ShelterId]);
		}

		private async Task<Dictionary<String, List<Models.File.File>>> CollectFiles(List<Data.Entities.Animal> animals, List<String> fileFields)
		{
            if (animals.Count == 0 || fileFields.Count == 0) return null;

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
            List<Models.File.File> fileDtos = await _builderFactory.Builder<FileBuilder>().Authorise(this._authorise).Build(files, fileFields);

            if (fileDtos == null || fileDtos.Count == 0) return null;

            Dictionary<String, Models.File.File> fileDtoMap = fileDtos.ToDictionary(x => x.Id);

			return animals.ToDictionary(
				animal => animal.Id,
				app => app.PhotosIds?.Select(fileId => fileDtoMap.TryGetValue(fileId, out Models.File.File fileDto) ? fileDto : null)
					.Where(fileDto => fileDto != null)
					.ToList() ?? null
			);
		}
	}
}