using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.MongoServices;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Data.Entities.EnumTypes;

namespace Pawfect_Messenger.Query.Queries
{
	public class ConversationQuery : BaseQuery<Data.Entities.Conversation>
	{

        public ConversationQuery
        (
            MongoDbService mongoDbService,
            IAuthorizationService authorizationService,
            ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver AuthorizationContentResolver

        ) : base(mongoDbService, authorizationService, AuthorizationContentResolver, claimsExtractor)
        {
        }

        // Λίστα με τα IDs των συνομιλιών για φιλτράρισμα
        public List<String> Ids { get; set; }

        public List<String> ExcludedIds { get; set; }

        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String> Participants { get; set; }

        public List<ConversationType> Types { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public ConversationQuery Authorise(AuthorizationFlags authorise) { _authorise = authorise; return this; }
        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Conversation> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Conversation>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Conversation> builder = Builders<Data.Entities.Conversation>.Filter;
            FilterDefinition<Data.Entities.Conversation> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των συνομιλιών
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Conversation.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.Conversation.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (Participants != null && Participants.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Participants.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Conversation.Participants), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (Types != null && Types.Any())
            {
                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.In(nameof(Data.Entities.Conversation.Type), this.Types);
            }

            // Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
            if (CreateFrom.HasValue)
			{
				filter &= builder.Gte(conversation => conversation.CreatedAt, CreateFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(conversation => conversation.CreatedAt, CreatedTill.Value);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Conversation>> ApplyAuthorization(FilterDefinition<Data.Entities.Conversation> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseConversations))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                FilterDefinition<BsonDocument> affiliatedFilter = await _authorizationContentResolver.BuildAffiliatedFilterParams(typeof(Data.Entities.Conversation));
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.Conversation));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            // Combine with the authorization filters (AND logic)
            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.Conversation>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.Conversation>(combinedFinalBsonFilter));
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
            if (fields == null || !fields.Any()) return new List<String>();

            HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				projectionFields.Add(nameof(Data.Entities.Conversation.Id));
                if (item.Equals(nameof(Models.Conversation.Conversation.Type))) projectionFields.Add(nameof(Data.Entities.Conversation.Type));
                if (item.Equals(nameof(Models.Conversation.Conversation.LastMessageAt))) projectionFields.Add(nameof(Data.Entities.Conversation.LastMessageAt));
                if (item.Equals(nameof(Models.Conversation.Conversation.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Conversation.CreatedAt));
				if (item.Equals(nameof(Models.Conversation.Conversation.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Conversation.UpdatedAt));

                if (item.StartsWith(nameof(Models.Conversation.Conversation.CreatedBy))) projectionFields.Add(nameof(Data.Entities.Conversation.CreatedBy));
                if (item.StartsWith(nameof(Models.Conversation.Conversation.LastMessagePreview))) projectionFields.Add(nameof(Data.Entities.Conversation.LastMessageId));
                if (item.StartsWith(nameof(Models.Conversation.Conversation.Participants))) projectionFields.Add(nameof(Data.Entities.Conversation.Participants));
			}
			return projectionFields.ToList();
		}
	}
}