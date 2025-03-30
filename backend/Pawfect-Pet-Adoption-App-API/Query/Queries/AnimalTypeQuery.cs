// FILE: Query/Queries/AnimalTypeQuery.cs
using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class AnimalTypeQuery : BaseQuery<AnimalType>
	{
		// Κατασκευαστής για την κλάση AnimalTypeQuery
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public AnimalTypeQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<AnimalType>();
		}

		// Λίστα από IDs τύπων ζώων για φιλτράρισμα
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Ονομασία τύπων ζώων για φιλτράρισμα
        public String? Name { get; set; }

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<AnimalType> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<AnimalType>> ApplyFilters()
		{
			FilterDefinitionBuilder<AnimalType> builder = Builders<AnimalType>.Filter;
			FilterDefinition<AnimalType> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των τύπων ζώων
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

            // Εφαρμόζει φίλτρο για την ονομασία των τύπων ζώων
            if (!String.IsNullOrEmpty(Name))
			{
				filter &= builder.Eq(animalType => animalType.Name, Name);
			}

			// Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : Name
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
			if (fields == null || !fields.Any() || fields.Contains("*")) return fields = EntityHelper.GetAllPropertyNames(typeof(AnimalTypeDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalTypeDto στα ονόματα πεδίων AnimalType
				projectionFields.Add(nameof(AnimalType.Id));
				if (item.Equals(nameof(AnimalTypeDto.Name))) projectionFields.Add(nameof(AnimalType.Name));
				if (item.Equals(nameof(AnimalTypeDto.Description))) projectionFields.Add(nameof(AnimalType.Description));
				if (item.Equals(nameof(AnimalTypeDto.CreatedAt))) projectionFields.Add(nameof(AnimalType.CreatedAt));
				if (item.Equals(nameof(AnimalTypeDto.UpdatedAt))) projectionFields.Add(nameof(AnimalType.UpdatedAt));
			}

			return projectionFields.ToList();
		}
	}
}