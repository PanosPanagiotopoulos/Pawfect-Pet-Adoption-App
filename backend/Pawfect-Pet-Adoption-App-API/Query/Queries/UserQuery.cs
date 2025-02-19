using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class UserQuery : BaseQuery<User>
	{
		// Κατασκευαστής για την κλάση UserQuery
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public UserQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<User>();
		}

		// Λίστα με τα IDs των χρηστών για φιλτράρισμα
		public List<String>? Ids { get; set; }

		// Λίστα με τα ονόματα των χρηστών για φιλτράρισμα
		public List<String>? FullNames { get; set; }

		// Λίστα με τους ρόλους των χρηστών για φιλτράρισμα
		public List<UserRole>? Roles { get; set; }

		// Λίστα με τις πόλεις των χρηστών για φιλτράρισμα
		public List<String>? Cities { get; set; }

		// Λίστα με τους ταχυδρομικούς κώδικες των χρηστών για φιλτράρισμα
		public List<String>? Zipcodes { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<User> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<User>> ApplyFilters()
		{
			FilterDefinitionBuilder<User> builder = Builders<User>.Filter;
			FilterDefinition<User> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των χρηστών
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τις λεπτομέρειες της αίτησης χρησιμοποιώντας regex
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Regex(user => user.FullName, new MongoDB.Bson.BsonRegularExpression(Query, "i"));
			}

			// Εφαρμόζει φίλτρο για τα ονόματα των χρηστών
			if (FullNames != null && FullNames.Any())
			{
				filter &= builder.In(user => user.FullName, FullNames);
			}

			// Εφαρμόζει φίλτρο για τους ρόλους των χρηστών
			if (Roles != null && Roles.Any())
			{
				filter &= builder.In(user => user.Role, Roles);
			}

			// Εφαρμόζει φίλτρο για τις πόλεις των χρηστών
			if (Cities != null && Cities.Any())
			{
				filter &= builder.In(user => user.Location.City, Cities);
			}

			// Εφαρμόζει φίλτρο για τους ταχυδρομικούς κώδικες των χρηστών
			if (Zipcodes != null && Zipcodes.Any())
			{
				filter &= builder.In(user => user.Location.ZipCode, Zipcodes);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreatedFrom.HasValue)
			{
				filter &= builder.Gte(user => user.CreatedAt, CreatedFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(user => user.CreatedAt, CreatedTill.Value);
			}

			// Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : FullName
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(UserDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων UserDto στα ονόματα πεδίων User
				projectionFields.Add(nameof(User.Id));
				if (item.Equals(nameof(UserDto.Email))) projectionFields.Add(nameof(User.Email));
				if (item.Equals(nameof(UserDto.FullName))) projectionFields.Add(nameof(User.FullName));
				if (item.Equals(nameof(UserDto.Role))) projectionFields.Add(nameof(User.Role));
				if (item.Equals(nameof(UserDto.Phone))) projectionFields.Add(nameof(User.Phone));
				if (item.Equals(nameof(UserDto.Location))) projectionFields.Add(nameof(User.Location));
				if (item.Equals(nameof(UserDto.AuthProvider))) projectionFields.Add(nameof(User.AuthProvider));
				if (item.Equals(nameof(UserDto.AuthProviderId))) projectionFields.Add(nameof(User.AuthProviderId));
				if (item.Equals(nameof(UserDto.IsVerified))) projectionFields.Add(nameof(User.IsVerified));
				if (item.Equals(nameof(UserDto.CreatedAt))) projectionFields.Add(nameof(User.CreatedAt));
				if (item.Equals(nameof(UserDto.UpdatedAt))) projectionFields.Add(nameof(User.UpdatedAt));
				if (item.StartsWith(nameof(UserDto.Shelter))) projectionFields.Add(nameof(User.ShelterId));
			}

			return projectionFields.ToList();
		}
	}
}