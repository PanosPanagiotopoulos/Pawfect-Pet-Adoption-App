using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoMessageBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Message
		public AutoMessageBuilder()
		{
			// Mapping για το Entity : Message σε Message για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Message, Message>();

			// POST Request Dto Μοντέλα
			CreateMap<Message, MessagePersist>();
			CreateMap<MessagePersist, Message>();
		}
	}

	public class MessageBuilder : BaseBuilder<MessageDto, Message>
	{
		private readonly UserLookup _userLookup;
		private readonly Lazy<IUserService> _userService;

		public MessageBuilder(UserLookup userLookup, Lazy<IUserService> userService)
		{
			_userLookup = userLookup;
			_userService = userService;
		}

		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<MessageDto, Message> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<MessageDto>> BuildDto(List<Message> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, List<UserDto>>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			List<MessageDto> result = new List<MessageDto>();
			foreach (Message e in entities)
			{
				MessageDto dto = new MessageDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Message.Content))) dto.Content = e.Content;
				if (nativeFields.Contains(nameof(Message.IsRead))) dto.IsRead = e.IsRead;
				if (nativeFields.Contains(nameof(Message.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.Sender = userMap[e.Id][0];
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.Recipient = userMap[e.Id][1];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, List<UserDto>>?> CollectUsers(List<Message> messages, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = messages.SelectMany(x => new[] { x.SenderId, x.RecipientId }).Distinct().ToList();

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

			// Ταίριασμα του προηγούμενου Dictionary με τα messages δημιουργώντας ένα Dictionary : [ MessageId -> UserId ] 
			return messages.ToDictionary(x => x.Id, x => new List<UserDto>() { userDtoMap[x.SenderId], userDtoMap[x.RecipientId] });
		}
	}
}