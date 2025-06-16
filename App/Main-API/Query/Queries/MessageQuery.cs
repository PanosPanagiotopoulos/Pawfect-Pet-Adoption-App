using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using Main_API.Data.Entities;
using Main_API.Data.Entities.Types.Authorization;
using Main_API.DevTools;
using Main_API.Models.Message;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.FilterServices;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Queries
{
	public class MessageQuery : BaseQuery<Data.Entities.Message>
	{
        public MessageQuery
		(
			MongoDbService mongoDbService,
			IAuthorizationService AuthorizationService,
			ClaimsExtractor claimsExtractor,
			IAuthorizationContentResolver AuthorizationContentResolver

		) : base(mongoDbService, AuthorizationService, AuthorizationContentResolver, claimsExtractor)
		{
		}

        // Λίστα από IDs μηνυμάτων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα από IDs συνομιλιών για φιλτράρισμα
        public List<String>? ConversationIds { get; set; }

		// Λίστα από IDs αποστολέων για φιλτράρισμα
		public List<String>? SenderIds { get; set; }

		// Λίστα από IDs παραληπτών για φιλτράρισμα
		public List<String>? RecipientIds { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public MessageQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Message> - ο ορισμός του φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Message>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Message> builder = Builders<Data.Entities.Message>.Filter;
            FilterDefinition<Data.Entities.Message> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για IDs μηνυμάτων
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Message.Id), referenceIds.Where(id => id != ObjectId.Empty));

            }

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Message.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για IDs συνομιλιών
            if (ConversationIds != null && ConversationIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ConversationIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Message.ConversationId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για IDs αποστολέων
			if (SenderIds != null && SenderIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = SenderIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Message.SenderId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για IDs παραληπτών
			if (RecipientIds != null && RecipientIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = RecipientIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Message.RecipientId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreateFrom.HasValue)
			{
				filter &= builder.Gte(message => message.CreatedAt, CreateFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(message => message.CreatedAt, CreatedTill.Value);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Message>> ApplyAuthorization(FilterDefinition<Data.Entities.Message> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseMessages))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                FilterDefinition<BsonDocument> affiliatedFilter = await _authorizationContentResolver.BuildAffiliatedFilterParams(typeof(Data.Entities.Message));
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.Message));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            // Combine with the authorization filters (AND logic)
            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.Message>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.Message>(combinedFinalBsonFilter));
        }

        // Επιστρέφει τα ονόματα των πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = [.. EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Message))];

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Χαρτογραφεί τα ονόματα των πεδίων του MessageDto στα ονόματα των πεδίων του Message
				projectionFields.Add(nameof(Data.Entities.Message.Id));
				if (item.Equals(nameof(Models.Message.Message.Content))) projectionFields.Add(nameof(Data.Entities.Message.Content));
				if (item.Equals(nameof(Models.Message.Message.IsRead))) projectionFields.Add(nameof(Data.Entities.Message.IsRead));
				if (item.Equals(nameof(Models.Message.Message.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Message.CreatedAt));
				if (item.StartsWith(nameof(Models.Message.Message.Conversation))) projectionFields.Add(nameof(Data.Entities.Message.ConversationId));
				if (item.StartsWith(nameof(Models.Message.Message.Sender))) projectionFields.Add(nameof(Data.Entities.Message.SenderId));
				if (item.StartsWith(nameof(Models.Message.Message.Recipient))) projectionFields.Add(nameof(Data.Entities.Message.RecipientId));
			}
			return projectionFields.ToList();
		}
	}
}