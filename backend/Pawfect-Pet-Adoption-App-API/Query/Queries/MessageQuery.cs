using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class MessageQuery : BaseQuery<Data.Entities.Message>
	{
        private readonly IFilterBuilder<Data.Entities.Message, Models.Lookups.MessageLookup> _filterBuilder;
        private readonly IQueryFactory _queryFactory;

        public MessageQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IFilterBuilder<Data.Entities.Message, Models.Lookups.MessageLookup> filterBuilder,
            IQueryFactory queryFactory

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor)
        {
            _filterBuilder = filterBuilder;
            _queryFactory = queryFactory;
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
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για IDs συνομιλιών
            if (ConversationIds != null && ConversationIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ConversationIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ConversationId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για IDs αποστολέων
			if (SenderIds != null && SenderIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = SenderIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("SenderId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για IDs παραληπτών
			if (RecipientIds != null && RecipientIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = RecipientIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("RecipientId", referenceIds.Where(id => id != ObjectId.Empty));
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

        public override async Task<FilterDefinition<Data.Entities.Message>> ApplyAuthorisation(FilterDefinition<Data.Entities.Message> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorisationService.AuthorizeAsync(Permission.BrowseMessages))
                    return filter;

            List<FilterDefinition<Data.Entities.Message>> authorizationFilters = new List<FilterDefinition<Data.Entities.Message>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                FilterDefinition<Data.Entities.Message> affiliatedFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<Data.Entities.Message>();
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<Data.Entities.Message> ownedFilter = _authorisationContentResolver.BuildOwnedFilterParams<Data.Entities.Message>();
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<Data.Entities.Message> combinedAuthorizationFilter = Builders<Data.Entities.Message>.Filter.Or(authorizationFilters);

            filter = Builders<Data.Entities.Message>.Filter.And(filter, combinedAuthorizationFilter);

			return await Task.FromResult(filter);
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