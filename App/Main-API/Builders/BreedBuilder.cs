using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AnimalTypeServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoBreedBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Breed
		public AutoBreedBuilder()
		{
            // Mapping για το Entity : Breed σε Breed για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.Breed, Data.Entities.Breed>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Breed, BreedPersist>();
            CreateMap<BreedPersist, Data.Entities.Breed>();
		}
	}

	public class BreedBuilder : BaseBuilder<Models.Breed.Breed, Data.Entities.Breed>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

		public BreedBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
		}

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        public BreedBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Breed.Breed>> Build(List<Data.Entities.Breed> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.AnimalType.AnimalType>? animalTypeMap = foreignEntitiesFields.ContainsKey(nameof(Models.AnimalType.AnimalType))
				? (await CollectAnimalTypes(entities,
									foreignEntitiesFields[nameof(Models.AnimalType.AnimalType)]))
				: null;

            List<Models.Breed.Breed> result = new List<Models.Breed.Breed>();
			foreach (Data.Entities.Breed e in entities)
			{
                Models.Breed.Breed dto = new Models.Breed.Breed();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Breed.Breed.Name))) dto.Name = e.Name;
				if (nativeFields.Contains(nameof(Models.Breed.Breed.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Models.Breed.Breed.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.Breed.Breed.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (nativeFields.Contains(nameof(Models.Breed.Breed.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (animalTypeMap != null && animalTypeMap.ContainsKey(e.Id)) dto.AnimalType = animalTypeMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.AnimalType.AnimalType>> CollectAnimalTypes(List<Data.Entities.Breed> breeds, List<String> animalTypeFields)
		{
            if (breeds.Count == 0 || animalTypeFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> animalTypeIds = [.. breeds.Select(x => x.AnimalTypeId).Distinct()];

            AnimalTypeLookup animalTypeLookup = new AnimalTypeLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalTypeLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalTypeLookup.PageSize = 1000;
            animalTypeLookup.Ids = animalTypeIds;
            animalTypeLookup.Fields = animalTypeFields;

            List<Data.Entities.AnimalType> animalTypes = await animalTypeLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.AnimalType.AnimalType> animalTypeDtos = await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(this._authorise).Build(animalTypes, animalTypeFields);

            if (animalTypeDtos == null || animalTypeDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AssetTypeId -> AssetTypeDto ]
            Dictionary<String, Models.AnimalType.AnimalType> animalTypeDtoMap = animalTypeDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα assets δημιουργώντας ένα Dictionary : [ AssetId -> AssetTypeId ] 
			return breeds.ToDictionary(x => x.Id, x => animalTypeDtoMap[x.AnimalTypeId]);
		}
	}
}