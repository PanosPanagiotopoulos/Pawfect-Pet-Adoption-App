﻿using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Breed;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class BreedQuery : BaseQuery<Breed>
	{
		// Constructor for the BreedQuery class
		// Input: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public BreedQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Breed>();
		}

		// Λίστα με τα αναγνωριστικά των φυλών για φιλτράρισμα
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα αναγνωριστικά των τύπων για φιλτράρισμα
        public List<String>? TypeIds { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

		// Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
		// Έξοδος: FilterDefinition<Breed> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
		protected override Task<FilterDefinition<Breed>> ApplyFilters()
		{
			FilterDefinitionBuilder<Breed> builder = Builders<Breed>.Filter;
			FilterDefinition<Breed> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα αναγνωριστικά των φυλών
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

            // Εφαρμόζει φίλτρο για τα αναγνωριστικά των τύπων
            if (TypeIds != null && TypeIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = TypeIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("AnimalTypeId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία έναρξης
			if (CreatedFrom.HasValue)
			{
				filter &= builder.Gte(breed => breed.CreatedAt, CreatedFrom.Value);
			}

			// Εφαρμόζει φίλτρο για την ημερομηνία λήξης
			if (CreatedTill.HasValue)
			{
				filter &= builder.Lte(breed => breed.CreatedAt, CreatedTill.Value);
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
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(BreedDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				projectionFields.Add(nameof(Breed.Id));
				if (item.Equals(nameof(BreedDto.Name))) projectionFields.Add(nameof(Breed.Name));
				if (item.Equals(nameof(BreedDto.Description))) projectionFields.Add(nameof(Breed.Description));
				if (item.Equals(nameof(BreedDto.CreatedAt))) projectionFields.Add(nameof(Breed.CreatedAt));
				if (item.Equals(nameof(BreedDto.UpdatedAt))) projectionFields.Add(nameof(Breed.UpdatedAt));
				if (item.StartsWith(nameof(BreedDto.AnimalType))) projectionFields.Add(nameof(Breed.TypeId));
			}
			return projectionFields.ToList();
		}
	}
}