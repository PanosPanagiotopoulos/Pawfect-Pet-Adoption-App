using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorization;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Notification;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class NotificationQuery : BaseQuery<Data.Entities.Notification>
	{
        public NotificationQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
        {
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
        public override Task<FilterDefinition<Data.Entities.Notification>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Notification> builder = Builders<Data.Entities.Notification>.Filter;
            FilterDefinition<Data.Entities.Notification> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των ειδοποιήσεων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Notification.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Notification.Id), referenceIds.Where(id => id != ObjectId.Empty));

            }
            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = UserIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Notification.UserId), referenceIds.Where(id => id != ObjectId.Empty));
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
        public override async Task<FilterDefinition<Data.Entities.Notification>> ApplyAuthorization(FilterDefinition<Data.Entities.Notification> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseNotifications))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.Notification));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            // Combine with the authorization filters (AND logic)
            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.Notification>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.Notification>(combinedFinalBsonFilter));
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Notification)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων NotificationDto στα ονόματα πεδίων Notification
				projectionFields.Add(nameof(Data.Entities.Notification.Id));
				if (item.Equals(nameof(Models.Notification.Notification.Type))) projectionFields.Add(nameof(Data.Entities.Notification.Type));
				if (item.Equals(nameof(Models.Notification.Notification.Content))) projectionFields.Add(nameof(Data.Entities.Notification.Content));
				if (item.Equals(nameof(Models.Notification.Notification.IsRead))) projectionFields.Add(nameof(Data.Entities.Notification.IsRead));
				if (item.Equals(nameof(Models.Notification.Notification.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Notification.CreatedAt));
				if (item.StartsWith(nameof(Models.Notification.Notification.User))) projectionFields.Add(nameof(Data.Entities.Notification.UserId));
			}
			return projectionFields.ToList();
		}
	}
}