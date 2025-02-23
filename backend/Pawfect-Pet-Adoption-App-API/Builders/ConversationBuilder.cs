using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Animal;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.User;
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
		private readonly UserLookup _userLookup;
		private readonly AnimalLookup _animalLookup;
		private readonly Lazy<IUserService> _userService;
		private readonly Lazy<IAnimalService> _animalService;

		public ConversationBuilder(UserLookup userLookup, AnimalLookup animalLookup, Lazy<IUserService> userService, Lazy<IAnimalService> animalService)
		{
			_userLookup = userLookup;
			_animalLookup = animalLookup;
			_userService = userService;
			_animalService = animalService;
		}

		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<ConversationDto, Conversation> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

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

			// Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
			_userLookup.Offset = LookupParams.Offset;
			// Γενική τιμή για τη λήψη των dtos
			_userLookup.PageSize = LookupParams.PageSize;
			_userLookup.SortDescending = LookupParams.SortDescending;
			_userLookup.Query = null;
			_userLookup.Ids = userIds;
			_userLookup.Fields = userFields;

			// Κατασκευή των dtos
			List<UserDto> userDtos = (await _userService.Value.QueryUsersAsync(_userLookup)).ToList();

			if (userDtos == null || userDtos.Count < 2) return null;

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
			Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις conversations δημιουργώντας ένα Dictionary : [ ConversationId -> List<UserDto> ] 
			return conversations.ToDictionary(x => x.Id, x => x.UserIds.Select(id => userDtoMap[id]).ToList());
		}

		private async Task<Dictionary<String, AnimalDto>> CollectAnimals(List<Conversation> conversations, List<String> animalFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> animalIds = conversations.Select(x => x.AnimalId).Distinct().ToList();

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

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ AnimalId -> AnimalDto ]
			Dictionary<String, AnimalDto> animalDtoMap = animalDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις conversations δημιουργώντας ένα Dictionary : [ ConversationId -> AnimalId ] 
			return conversations.ToDictionary(x => x.Id, x => animalDtoMap[x.AnimalId]);
		}
	}
}