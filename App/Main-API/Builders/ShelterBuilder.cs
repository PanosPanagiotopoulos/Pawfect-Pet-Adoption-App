using AutoMapper;

using Main_API.Data.Entities;
using Main_API.Data.Entities.HelperModels;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.Models.AdoptionApplication;
using Main_API.Models.Animal;
using Main_API.Models.Lookups;
using Main_API.Models.Shelter;
using Main_API.Models.User;
using Main_API.Query;
using Main_API.Services.AnimalServices;
using Main_API.Services.UserServices;

namespace Main_API.Builders
{
	public class AutoShelterBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Shelter
		public AutoShelterBuilder()
		{
			// Mapping για nested object : OpeningHours
			CreateMap<OperatingHours, OperatingHours>();
			// Mapping για nested object : SocialMedia
			CreateMap<SocialMedia, SocialMedia>();

            // Mapping για το Entity : Shelter σε Shelter για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.Shelter, Data.Entities.Shelter>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Shelter, ShelterPersist>();
            CreateMap<ShelterPersist, Data.Entities.Shelter>();
		}
	}

	public class ShelterBuilder : BaseBuilder<Models.Shelter.Shelter, Data.Entities.Shelter>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ShelterBuilder
		(
		  IQueryFactory queryFactory,
          IBuilderFactory builderFactory

        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public ShelterBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }


        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Shelter.Shelter>> Build(List<Data.Entities.Shelter> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.User.User>? userMap = foreignEntitiesFields.ContainsKey(nameof(Models.Shelter.Shelter.User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(Models.Shelter.Shelter.User)]))
				: null;

            Dictionary<String, List<Models.Animal.Animal>>? animalsMap = foreignEntitiesFields.ContainsKey(nameof(Models.Shelter.Shelter.Animals))
				? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Models.Shelter.Shelter.Animals)]))
				: null;

            Dictionary<String, List<Models.AdoptionApplication.AdoptionApplication>>? adoptionApplicationsMap = foreignEntitiesFields.ContainsKey(nameof(Models.Shelter.Shelter.ReceivedAdoptionApplications))
               ? (await CollectAdoptionApplications(entities, foreignEntitiesFields[nameof(Models.Shelter.Shelter.ReceivedAdoptionApplications)]))
               : null;

            List<Models.Shelter.Shelter> result = new List<Models.Shelter.Shelter>();
			foreach (Data.Entities.Shelter e in entities)
			{
                Models.Shelter.Shelter dto = new Models.Shelter.Shelter();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.ShelterName))) dto.ShelterName = e.ShelterName;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.Website))) dto.Website = e.Website;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.SocialMedia))) dto.SocialMedia = e.SocialMedia;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.OperatingHours))) dto.OperatingHours = e.OperatingHours;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.VerificationStatus))) dto.VerificationStatus = e.VerificationStatus;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.VerifiedBy))) dto.VerifiedBy = e.VerifiedById;

				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];
				if (animalsMap != null && animalsMap.ContainsKey(e.Id)) dto.Animals = animalsMap[e.Id];
                if (adoptionApplicationsMap != null && adoptionApplicationsMap.ContainsKey(e.Id)) dto.ReceivedAdoptionApplications = adoptionApplicationsMap[e.Id];

                result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.User.User>?> CollectUsers(List<Data.Entities.Shelter> shelters, List<String> userFields)
		{
            if (shelters.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. shelters.Select(x => x.UserId).Distinct()];

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
			return shelters.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}

		private async Task<Dictionary<String, List<Models.Animal.Animal>>?> CollectAnimals(List<Data.Entities.Shelter> shelters, List<String> animalFields)
		{
            if (shelters.Count == 0 || animalFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> shelterIds = [.. shelters.Select(x => x.Id).Distinct()];

            AnimalLookup animalLookup = new AnimalLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalLookup.PageSize = 100000;
            animalLookup.ShelterIds = shelterIds;
            animalLookup.Fields = animalFields;

            List<Data.Entities.Animal> animals = await animalLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<Models.Animal.Animal> animalDtos = await _builderFactory.Builder<AnimalBuilder>().Authorise(this._authorise).Build(animals, animalFields);

            if (animalDtos == null || animalDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
            Dictionary<String, Models.Animal.Animal> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> List<AnimalId> ] 
			return animalDtos.GroupBy(a => a.Shelter!.Id).ToDictionary(g => g.Key, g => g.ToList());
		}

        private async Task<Dictionary<String, List<Models.AdoptionApplication.AdoptionApplication>>> CollectAdoptionApplications(List<Data.Entities.Shelter> shelters, List<String> adoptionApplicationFields)
        {
            if (shelters.Count == 0 || adoptionApplicationFields.Count == 0) return null;

            AdoptionApplicationLookup animalLookup = new AdoptionApplicationLookup();
            animalLookup.Offset = 1;
            animalLookup.PageSize = 100000;
            animalLookup.ShelterIds = [.. shelters.Select(x => x.Id)];
            animalLookup.Fields = adoptionApplicationFields;

            List<Data.Entities.AdoptionApplication> adoptionApplications = await animalLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.AdoptionApplication.AdoptionApplication> adoptionApplicationDtos = await _builderFactory.Builder<AdoptionApplicationBuilder>().Authorise(this._authorise).Build(adoptionApplications, adoptionApplicationFields);

            if (adoptionApplicationDtos == null || adoptionApplicationDtos.Count == 0) { return null; }

            Dictionary<String, Models.AdoptionApplication.AdoptionApplication> adoptionApplicationDtoMap = adoptionApplicationDtos.ToDictionary(x => x.Id);

            return adoptionApplicationDtos.GroupBy(a => a.Shelter!.Id).ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}