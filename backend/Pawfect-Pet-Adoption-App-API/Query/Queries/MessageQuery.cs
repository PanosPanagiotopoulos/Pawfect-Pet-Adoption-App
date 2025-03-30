using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Message;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class MessageQuery : BaseQuery<Message>
	{
		// Constructor για την κλάση MessageQuery
		// Είσοδος: mongoDbService - μια παρουσία της κλάσης MongoDbService
		public MessageQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Message>();
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

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<Message> - ο ορισμός του φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<Message>> ApplyFilters()
		{
			FilterDefinitionBuilder<Message> builder = Builders<Message>.Filter;
			FilterDefinition<Message> filter = builder.Empty;

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

		// Επιστρέφει τα ονόματα των πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(MessageDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Χαρτογραφεί τα ονόματα των πεδίων του MessageDto στα ονόματα των πεδίων του Message
				projectionFields.Add(nameof(Message.Id));
				if (item.Equals(nameof(MessageDto.Content))) projectionFields.Add(nameof(Message.Content));
				if (item.Equals(nameof(MessageDto.IsRead))) projectionFields.Add(nameof(Message.IsRead));
				if (item.Equals(nameof(MessageDto.CreatedAt))) projectionFields.Add(nameof(Message.CreatedAt));
				if (item.StartsWith(nameof(MessageDto.Conversation))) projectionFields.Add(nameof(Message.ConversationId));
				if (item.StartsWith(nameof(MessageDto.Sender))) projectionFields.Add(nameof(Message.SenderId));
				if (item.StartsWith(nameof(MessageDto.Recipient))) projectionFields.Add(nameof(Message.RecipientId));
			}
			return projectionFields.ToList();
		}
	}
}