using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.AnimalServices;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoConversationBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Conversation
		public AutoConversationBuilder()
		{
			// Mapping για το Entity : Conversation σε Conversation για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Conversation, Conversation>();

			// POST Request Dto Μοντέλα
			CreateMap<Conversation, ConversationPersist>();
			CreateMap<ConversationPersist, Conversation>();
		}
	}

	public class ConversationBuilder : BaseBuilder<ConversationDto, Conversation>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ConversationBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public ConversationBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }


		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<ConversationDto>> BuildDto(List<Conversation> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, List<UserDto>>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			Dictionary<String, AnimalDto>? animalMap = foreignEntitiesFields.ContainsKey(nameof(Animal))
				? (await CollectAnimals(entities, foreignEntitiesFields[nameof(Animal)]))
				: null;

			List<ConversationDto> result = new List<ConversationDto>();
			foreach (Conversation e in entities)
			{
				ConversationDto dto = new ConversationDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Conversation.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Conversation.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.Users = userMap[e.Id];
				if (animalMap != null && animalMap.ContainsKey(e.Id)) dto.Animal = animalMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, List<UserDto>>?> CollectUsers(List<Conversation> conversations, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = conversations.SelectMany(x => x.UserIds).Distinct().ToList();

			UserLookup userLookup = new UserLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<UserDto> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).BuildDto(users, userFields);

            if (userDtos == null || !userDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις conversations δημιουργώντας ένα Dictionary : [ ConversationId -> List<UserDto> ] 
			return conversations.ToDictionary(x => x.Id, x => x.UserIds.Select(id => userDtoMap[id]).ToList());
		}

		private async Task<Dictionary<String, AnimalDto>> CollectAnimals(List<Conversation> conversations, List<String> animalFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalIds = conversations.Select(x => x.AnimalId).Distinct().ToList();

            AnimalLookup animalLookup = new AnimalLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            animalLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            animalLookup.PageSize = 1000;
            animalLookup.Ids = animalIds;
            animalLookup.Fields = animalFields;

            List<Data.Entities.Animal> animals = await animalLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            // Κατασκευή των dtos
            List<AnimalDto> animalDtos = await _builderFactory.Builder<AnimalBuilder>().Authorise(this._authorise).BuildDto(animals, animalFields);

            if (animalDtos == null || !animalDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
            Dictionary<String, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις conversations δημιουργώντας ένα Dictionary : [ ConversationId -> AnimalId ] 
			return conversations.ToDictionary(x => x.Id, x => animalDtoMap[x.AnimalId]);
		}
	}
}