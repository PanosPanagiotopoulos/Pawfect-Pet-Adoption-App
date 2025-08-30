using AutoMapper;
using Pawfect_Notifications.Data.Entities.Types.Authorization;
using Pawfect_Notifications.Models.Lookups;
using Pawfect_Notifications.Models.Notification;
using Pawfect_Notifications.Query;

namespace Pawfect_Notifications.Builders
{
	public class AutoNotificationBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Notification
		public AutoNotificationBuilder()
		{
            // Mapping για το Entity : Notification σε Notification για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Data.Entities.Notification, Data.Entities.Notification>();

            // POST Request Dto Μοντέλα
            CreateMap<Data.Entities.Notification, NotificationEvent>();
            CreateMap<NotificationEvent, Data.Entities.Notification>();
		}
	}

	public class NotificationBuilder : BaseBuilder<Models.Notification.Notification, Data.Entities.Notification>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;

        public NotificationBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        public NotificationBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }


		// Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
		public override async Task<List<Models.Notification.Notification>> Build(List<Data.Entities.Notification> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = this.ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.User.User>? userMap = foreignEntitiesFields.ContainsKey(nameof(Models.Notification.Notification.User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(Models.Notification.Notification.User)]))
				: null;

            List<Models.Notification.Notification> result = new List<Models.Notification.Notification>();
			foreach (Data.Entities.Notification e in entities)
			{
                Models.Notification.Notification dto = new Models.Notification.Notification();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Notification.Notification.Title))) dto.Title = e.Title;
                if (nativeFields.Contains(nameof(Models.Notification.Notification.Content))) dto.Content = e.Content;
                if (nativeFields.Contains(nameof(Models.Notification.Notification.IsRead))) dto.IsRead = e.IsRead;
				if (nativeFields.Contains(nameof(Models.Notification.Notification.CreatedAt))) dto.CreatedAt = e.CreatedAt;
                if (nativeFields.Contains(nameof(Models.Notification.Notification.ReadAt))) dto.ReadAt = e.ReadAt;
                if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.User.User>> CollectUsers(List<Data.Entities.Notification> notifications, List<String> userFields)
		{
            if (notifications.Count == 0 || notifications.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. notifications.Select(x => x.UserId).Distinct()];

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 10000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τις notifications δημιουργώντας ένα Dictionary : [ NotificationId -> UserId ] 
			return notifications.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}
	}
}