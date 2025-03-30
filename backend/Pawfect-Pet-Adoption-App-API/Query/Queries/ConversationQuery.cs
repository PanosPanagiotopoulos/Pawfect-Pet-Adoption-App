using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Conversation;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class ConversationQuery : BaseQuery<Conversation>
	{
		// Constructor for the ConversationQuery class
		// Input: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public ConversationQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Conversation>();
		}

		// Λίστα με τα IDs των συνομιλιών για φιλτράρισμα
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

		// Λίστα με τα IDs των ζώων για φιλτράρισμα
		public List<String>? AnimalIds { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreateFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<Conversation> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<Conversation>> ApplyFilters()
		{
			FilterDefinitionBuilder<Conversation> builder = Builders<Conversation>.Filter;
			FilterDefinition<Conversation> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των συνομιλιών
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

			// Εφαρμόζει φίλτρο για τα IDs των ζώων
			if (AnimalIds != null && AnimalIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = AnimalIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("AnimalId", referenceIds.Where(id => id != ObjectId.Empty));
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

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(ConversationDto)).ToList();
			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				projectionFields.Add(nameof(Conversation.Id));
				if (item.Equals(nameof(ConversationDto.CreatedAt))) projectionFields.Add(nameof(Conversation.CreatedAt));
				if (item.Equals(nameof(ConversationDto.UpdatedAt))) projectionFields.Add(nameof(Conversation.UpdatedAt));
				if (item.StartsWith(nameof(ConversationDto.Users))) projectionFields.Add(nameof(Conversation.UserIds));
				if (item.StartsWith(nameof(ConversationDto.Animal))) projectionFields.Add(nameof(Conversation.AnimalId));

			}
			return projectionFields.ToList();
		}
	}
}