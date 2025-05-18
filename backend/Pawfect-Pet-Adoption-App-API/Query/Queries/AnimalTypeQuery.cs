
using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class AnimalTypeQuery : BaseQuery<Data.Entities.AnimalType>
	{
        private readonly IFilterBuilder<Data.Entities.AnimalType, Models.Lookups.AnimalTypeLookup> _filterBuilder;

        public AnimalTypeQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IFilterBuilder<Data.Entities.AnimalType, Models.Lookups.AnimalTypeLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor)
        {
            _filterBuilder = filterBuilder;
        }

        // Λίστα από IDs τύπων ζώων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Ονομασία τύπων ζώων για φιλτράρισμα
        public String? Name { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;
        public AnimalTypeQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<AnimalType> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.AnimalType>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.AnimalType> builder = Builders<Data.Entities.AnimalType>.Filter;
            FilterDefinition<Data.Entities.AnimalType> filter = builder.Empty;

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

        public override async Task<FilterDefinition<Data.Entities.AnimalType>> ApplyAuthorisation(FilterDefinition<Data.Entities.AnimalType> filter)
        {
            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) return fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.AnimalType)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalTypeDto στα ονόματα πεδίων AnimalType
				projectionFields.Add(nameof(Data.Entities.AnimalType.Id));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.Name))) projectionFields.Add(nameof(Data.Entities.AnimalType.Name));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.Description))) projectionFields.Add(nameof(Data.Entities.AnimalType.Description));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.CreatedAt))) projectionFields.Add(nameof(Data.Entities.AnimalType.CreatedAt));
				if (item.Equals(nameof(Models.AnimalType.AnimalType.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.AnimalType.UpdatedAt));
			}

			return projectionFields.ToList();
		}
	}
}