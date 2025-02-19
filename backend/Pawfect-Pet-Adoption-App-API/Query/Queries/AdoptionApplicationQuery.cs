using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;


namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class AdoptionApplicationQuery : BaseQuery<AdoptionApplication>
	{

		// Κατασκευαστής για την κλάση AdoptionApplicationQuery
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public AdoptionApplicationQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<AdoptionApplication>();
		}

		// Λίστα με τα IDs των αιτήσεων υιοθεσίας για φιλτράρισμα
		public List<String>? Ids { get; set; }

		// Λίστα με τα IDs των χρηστών για φιλτράρισμα
		public List<String>? UserIds { get; set; }

		// Λίστα με τα IDs των ζώων για φιλτράρισμα
		public List<String>? AnimalIds { get; set; }

		// Λίστα με τα IDs των καταφυγίων για φιλτράρισμα
		public List<String>? ShelterIds { get; set; }

		// Λίστα με τα καταστήματα υιοθεσίας για φιλτράρισμα
		public List<AdoptionStatus>? Status { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<AdoptionApplication> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<AdoptionApplication>> ApplyFilters()
		{
			FilterDefinitionBuilder<AdoptionApplication> builder = Builders<AdoptionApplication>.Filter;
			FilterDefinition<AdoptionApplication> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των αιτήσεων υιοθεσίας
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
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

			// Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
			if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("ShelterId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα καταστήματα υιοθεσίας
			if (Status != null && Status.Any())
			{
				filter &= builder.In(nameof(AdoptionApplication.Status), Status);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreatedFrom.HasValue)
			{
				filter &= builder.Gte(asset => asset.CreatedAt, CreatedFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(asset => asset.CreatedAt, CreatedTill.Value);
			}

			return Task.FromResult(filter);
		}

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(AdoptionApplicationDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AdoptionApplicationDto στα ονόματα πεδίων AdoptionApplication
				projectionFields.Add(nameof(AdoptionApplication.Id));
				if (item.Equals(nameof(AdoptionApplicationDto.Status))) projectionFields.Add(nameof(AdoptionApplication.Status));
				if (item.Equals(nameof(AdoptionApplicationDto.ApplicationDetails))) projectionFields.Add(nameof(AdoptionApplication.ApplicationDetails));
				if (item.Equals(nameof(AdoptionApplicationDto.CreatedAt))) projectionFields.Add(nameof(AdoptionApplication.CreatedAt));
				if (item.Equals(nameof(AdoptionApplicationDto.UpdatedAt))) projectionFields.Add(nameof(AdoptionApplication.UpdatedAt));
				if (item.StartsWith(nameof(AdoptionApplicationDto.User))) projectionFields.Add(nameof(AdoptionApplication.UserId));
				if (item.StartsWith(nameof(AdoptionApplicationDto.Animal))) projectionFields.Add(nameof(AdoptionApplication.AnimalId));
				if (item.StartsWith(nameof(AdoptionApplicationDto.Shelter))) projectionFields.Add(nameof(AdoptionApplication.ShelterId));
			}
			return projectionFields.ToList();
		}
	}
}
