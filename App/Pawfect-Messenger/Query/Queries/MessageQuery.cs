using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.DevTools;
using Pawfect_Messenger.Services.AuthenticationServices;
using Pawfect_Messenger.Services.MongoServices;

namespace Pawfect_Messenger.Query.Queries
{
	public class MessageQuery : BaseQuery<Data.Entities.Message>
	{
        public MessageQuery
		(
			MongoDbService mongoDbService,
			IAuthorizationService AuthorizationService,
			ClaimsExtractor claimsExtractor,
			IAuthorizationContentResolver authorizationContentResolver

		) : base(mongoDbService, AuthorizationService, authorizationContentResolver, claimsExtractor)
		{
		}

        // Λίστα από IDs μηνυμάτων για φιλτράρισμα
        public List<String> Ids { get; set; }

        public List<String> ExcludedIds { get; set; }

        // Λίστα από IDs συνομιλιών για φιλτράρισμα
        public List<String> ConversationIds { get; set; }

		// Λίστα από IDs αποστολέων για φιλτράρισμα
		public List<String> SenderIds { get; set; }

		// Λίστα από IDs παραληπτών για φιλτράρισμα
		public List<MessageType> Types { get; set; }
        public List<String> ReadyBy { get; set; }
        public List<MessageStatus> Statuses { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public MessageQuery Authorise(AuthorizationFlags authorise) { _authorise = authorise; return this; }

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

			if (SenderIds != null && SenderIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = SenderIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.Message.SenderId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			if (Types != null && Types.Any())
			{
				filter &= builder.In(nameof(Data.Entities.Message.Type), Types);
			}

            if (ReadyBy != null && ReadyBy.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ReadyBy.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.In(nameof(Data.Entities.Message.ReadBy), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για IDs παραληπτών
            if (Statuses != null && Statuses.Any())
            {
                filter &= builder.In(nameof(Data.Entities.Message.Status), Statuses);
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
            if (fields == null || !fields.Any()) return new List<String>();

            HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				projectionFields.Add(nameof(Data.Entities.Message.Id));
				if (item.Equals(nameof(Models.Message.Message.Content))) projectionFields.Add(nameof(Data.Entities.Message.Content));
                if (item.Equals(nameof(Models.Message.Message.Type))) projectionFields.Add(nameof(Data.Entities.Message.Type));
                if (item.Equals(nameof(Models.Message.Message.Status))) projectionFields.Add(nameof(Data.Entities.Message.Status));
                if (item.Equals(nameof(Models.Message.Message.CreatedAt))) projectionFields.Add(nameof(Data.Entities.Message.CreatedAt));
                if (item.Equals(nameof(Models.Message.Message.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.Message.UpdatedAt));

                if (item.StartsWith(nameof(Models.Message.Message.Conversation))) projectionFields.Add(nameof(Data.Entities.Message.ConversationId));
				if (item.StartsWith(nameof(Models.Message.Message.Sender))) projectionFields.Add(nameof(Data.Entities.Message.SenderId));
				if (item.StartsWith(nameof(Models.Message.Message.ReadBy))) projectionFields.Add(nameof(Data.Entities.Message.ReadBy));
			}
			return projectionFields.ToList();
		}
	}
}