using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
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
			CreateMap<Breed, Breed>();

			// POST Request Dto Μοντέλα
			CreateMap<Breed, BreedPersist>();
			CreateMap<BreedPersist, Breed>();
		}
	}

	public class BreedBuilder : BaseBuilder<BreedDto, Breed>
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
        public override async Task<List<BreedDto>> BuildDto(List<Breed> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, AnimalTypeDto>? animalTypeMap = foreignEntitiesFields.ContainsKey(nameof(AnimalType))
				? (await CollectAnimalTypes(entities,
									foreignEntitiesFields[nameof(AnimalType)]))
				: null;

			List<BreedDto> result = new List<BreedDto>();
			foreach (Breed e in entities)
			{
				BreedDto dto = new BreedDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Breed.Name))) dto.Name = e.Name;
				if (nativeFields.Contains(nameof(Breed.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Breed.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Breed.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (nativeFields.Contains(nameof(Breed.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (animalTypeMap != null && animalTypeMap.ContainsKey(e.Id)) dto.AnimalType = animalTypeMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, AnimalTypeDto>> CollectAnimalTypes(List<Breed> breeds, List<String> animalTypeFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalTypeIds = breeds.Select(x => x.TypeId).Distinct().ToList();

            AnimalTypeLookup animalTypeLookup = new AnimalTypeLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalTypeLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalTypeLookup.PageSize = 1000;
            animalTypeLookup.Ids = animalTypeIds;
            animalTypeLookup.Fields = animalTypeFields;

            List<Data.Entities.AnimalType> animalTypes = await animalTypeLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<AnimalTypeDto> animalTypeDtos = await _builderFactory.Builder<AnimalTypeBuilder>().Authorise(this._authorise).BuildDto(animalTypes, animalTypeFields);

            if (animalTypeDtos == null || !animalTypeDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο Guid ως κλειδί και το "Dto model" ως τιμή : [ AssetTypeId -> AssetTypeDto ]
            Dictionary<String, AnimalTypeDto> animalTypeDtoMap = animalTypeDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα assets δημιουργώντας ένα Dictionary : [ AssetId -> AssetTypeId ] 
			return breeds.ToDictionary(x => x.Id, x => animalTypeDtoMap[x.TypeId]);
		}
	}
}