using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class NotificationQuery : BaseQuery<Notification>
	{
		// Κατασκευαστής για την κλάση NotificationQuery
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public NotificationQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Notification>();
		}

		// Λίστα με τα IDs των ειδοποιήσεων για φιλτράρισμα
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

		// Λίστα με τους τύπους ειδοποιήσεων για φιλτράρισμα
		public List<NotificationType>? NotificationTypes { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public NotificationQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Notification> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Notification>> ApplyFilters()
		{
			FilterDefinitionBuilder<Notification> builder = Builders<Notification>.Filter;
			FilterDefinition<Notification> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των ειδοποιήσεων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));

            }
            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = UserIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("UserId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τους τύπους ειδοποιήσεων
			if (NotificationTypes != null && NotificationTypes.Any())
			{
				filter &= builder.In(notification => notification.Type, NotificationTypes);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreateFrom.HasValue)
			{
				filter &= builder.Gte(notification => notification.CreatedAt, CreateFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(notification => notification.CreatedAt, CreatedTill.Value);
			}

			return Task.FromResult(filter);
		}
		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(NotificationDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων NotificationDto στα ονόματα πεδίων Notification
				projectionFields.Add(nameof(Notification.Id));
				if (item.Equals(nameof(NotificationDto.Type))) projectionFields.Add(nameof(Notification.Type));
				if (item.Equals(nameof(NotificationDto.Content))) projectionFields.Add(nameof(Notification.Content));
				if (item.Equals(nameof(NotificationDto.IsRead))) projectionFields.Add(nameof(Notification.IsRead));
				if (item.Equals(nameof(NotificationDto.CreatedAt))) projectionFields.Add(nameof(Notification.CreatedAt));
				if (item.StartsWith(nameof(NotificationDto.User))) projectionFields.Add(nameof(Notification.UserId));
			}
			return projectionFields.ToList();
		}
	}
}