using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.Shelter;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class ShelterQuery : BaseQuery<Data.Entities.Shelter>
	{
        private readonly IFilterBuilder<Data.Entities.Shelter, Models.Lookups.ShelterLookup> _filterBuilder;

        public ShelterQuery
        (
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
            IHttpContextAccessor httpContextAccessor,
            IFilterBuilder<Data.Entities.Shelter, Models.Lookups.ShelterLookup> filterBuilder

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor, httpContextAccessor)
        {
            _filterBuilder = filterBuilder;
        }

        // Λίστα με τα IDs των καταφυγίων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα με τα IDs των χρηστών για φιλτράρισμα
        public List<String>? UserIds { get; set; }

		// Λίστα με τις καταστάσεις επιβεβαίωσης για φιλτράρισμα
		public List<VerificationStatus>? VerificationStatuses { get; set; }

		// Λίστα με τα IDs των admin που επιβεβαίωσαν για φιλτράρισμα
		public List<String>? VerifiedBy { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public ShelterQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<Shelter> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.Shelter>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.Shelter> builder = Builders<Data.Entities.Shelter>.Filter;
            FilterDefinition<Data.Entities.Shelter> filter = builder.Empty;

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

			// Εφαρμόζει φίλτρο για τις καταστάσεις επιβεβαίωσης
			if (VerificationStatuses != null && VerificationStatuses.Any())
			{
				filter &= builder.In(shelter => shelter.VerificationStatus, VerificationStatuses);
			}

			// Εφαρμόζει φίλτρο για τα IDs των admin που επιβεβαίωσαν
			if (VerifiedBy != null && VerifiedBy.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = VerifiedBy.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("VerifiedBy", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : ShelterName
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

        public override async Task<FilterDefinition<Data.Entities.Shelter>> ApplyAuthorisation(FilterDefinition<Data.Entities.Shelter> filter)
        {
            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.Shelter)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων ShelterDto στα ονόματα πεδίων Shelter
				projectionFields.Add(nameof(Data.Entities.Shelter.Id));
				if (item.StartsWith(nameof(Models.Shelter.Shelter.User))) projectionFields.Add(nameof(Data.Entities.Shelter.UserId));
				if (item.Equals(nameof(Models.Shelter.Shelter.ShelterName))) projectionFields.Add(nameof(Data.Entities.Shelter.ShelterName));
				if (item.Equals(nameof(Models.Shelter.Shelter.Description))) projectionFields.Add(nameof(Data.Entities.Shelter.Description));
				if (item.Equals(nameof(Models.Shelter.Shelter.Website))) projectionFields.Add(nameof(Data.Entities.Shelter.Website));
				if (item.Equals(nameof(Models.Shelter.Shelter.SocialMedia))) projectionFields.Add(nameof(Data.Entities.Shelter.SocialMedia));
				if (item.Equals(nameof(Models.Shelter.Shelter.OperatingHours))) projectionFields.Add(nameof(Data.Entities.Shelter.OperatingHours));
				if (item.Equals(nameof(Models.Shelter.Shelter.VerificationStatus))) projectionFields.Add(nameof(Data.Entities.Shelter.VerificationStatus));
				if (item.Equals(nameof(Models.Shelter.Shelter.VerifiedBy))) projectionFields.Add(nameof(Data.Entities.Shelter.VerifiedById));
			}
			return projectionFields.ToList();
		}
	}
}