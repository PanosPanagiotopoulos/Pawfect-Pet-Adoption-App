using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoNotificationBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Notification
		public AutoNotificationBuilder()
		{
			// Mapping για το Entity : Notification σε Notification για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Notification, Notification>();

			// POST Request Dto Μοντέλα
			CreateMap<Notification, NotificationPersist>();
			CreateMap<NotificationPersist, Notification>();
		}
	}

	public class NotificationBuilder : BaseBuilder<NotificationDto, Notification>
	{
		private readonly UserLookup _userLookup;
		private readonly Lazy<IUserService> _userService;

		public NotificationBuilder(UserLookup userLookup, Lazy<IUserService> userService)
		{
			_userLookup = userLookup;
			_userService = userService;
		}

		// Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
		public override BaseBuilder<NotificationDto, Notification> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<NotificationDto>> BuildDto(List<Notification> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, UserDto>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			List<NotificationDto> result = new List<NotificationDto>();
			foreach (Notification e in entities)
			{
				NotificationDto dto = new NotificationDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Notification.Content))) dto.Content = e.Content;
				if (nativeFields.Contains(nameof(Notification.IsRead))) dto.IsRead = e.IsRead;
				if (nativeFields.Contains(nameof(Notification.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, UserDto>> CollectUsers(List<Notification> notifications, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = notifications.Select(x => x.UserId).Distinct().ToList();

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

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
			Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις notifications δημιουργώντας ένα Dictionary : [ NotificationId -> UserId ] 
			return notifications.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}
	}
}