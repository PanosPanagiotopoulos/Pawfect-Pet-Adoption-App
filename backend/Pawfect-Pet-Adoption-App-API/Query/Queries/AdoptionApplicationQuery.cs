using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Exceptions;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.Convention;
using Pawfect_Pet_Adoption_App_API.Services.FilterServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;
using System.Security.Claims;


namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class AdoptionApplicationQuery : BaseQuery<Data.Entities.AdoptionApplication>
	{
        private readonly IFilterBuilder<Data.Entities.AdoptionApplication, AdoptionApplicationLookup> _filterBuilder;
        private readonly IConventionService _conventionService;

        public AdoptionApplicationQuery
		(
			MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
			ClaimsExtractor claimsExtractor,
            IAuthorisationContentResolver authorisationContentResolver,
			IFilterBuilder<Data.Entities.AdoptionApplication, Models.Lookups.AdoptionApplicationLookup> filterBuilder,
			IConventionService conventionService

        ) : base(mongoDbService, authorisationService, authorisationContentResolver, claimsExtractor)
        {
            _filterBuilder = filterBuilder;
            _conventionService = conventionService;
        }

        // Λίστα με τα IDs των αιτήσεων υιοθεσίας για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }

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

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public AdoptionApplicationQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<AdoptionApplication> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override Task<FilterDefinition<Data.Entities.AdoptionApplication>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.AdoptionApplication> builder = Builders<Data.Entities.AdoptionApplication>.Filter;
            FilterDefinition<Data.Entities.AdoptionApplication> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των αιτήσεων υιοθεσίας
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
				filter &= builder.In(nameof(Data.Entities.AdoptionApplication.Status), Status);
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

        public override async Task<FilterDefinition<Data.Entities.AdoptionApplication>> ApplyAuthorisation(
            FilterDefinition<Data.Entities.AdoptionApplication> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorisationService.AuthorizeAsync(Permission.BrowseAdoptionApplications))
                    return filter;

            List<FilterDefinition<Data.Entities.AdoptionApplication>> authorizationFilters = new List<FilterDefinition<Data.Entities.AdoptionApplication>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                ClaimsPrincipal claimsPrincipal = _authorisationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (!_conventionService.IsValidId(userId)) throw new UnAuthenticatedException("No authenticated user found");
                String userShelterId = await _authorisationContentResolver.CurrentPrincipalShelter();

				if (!_conventionService.IsValidId(userShelterId))
                    throw new UnAuthenticatedException("No authenticated shelter found");

                FilterDefinition<Data.Entities.AdoptionApplication> affiliatedFilter = _authorisationContentResolver.BuildAffiliatedFilterParams<Data.Entities.AdoptionApplication>(userShelterId);
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<Data.Entities.AdoptionApplication> ownedFilter = _authorisationContentResolver.BuildOwnedFilterParams<Data.Entities.AdoptionApplication>();
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<Data.Entities.AdoptionApplication> combinedAuthorizationFilter = Builders<Data.Entities.AdoptionApplication>.Filter.Or(authorizationFilters);

            filter = Builders<Data.Entities.AdoptionApplication>.Filter.And(filter, combinedAuthorizationFilter);

            return await Task.FromResult(filter);
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) fields = EntityHelper.GetAllPropertyNames(typeof(Data.Entities.AdoptionApplication)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AdoptionApplicationDto στα ονόματα πεδίων AdoptionApplication
				projectionFields.Add(nameof(Data.Entities.AdoptionApplication.Id));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.Status))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.Status));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.ApplicationDetails))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.ApplicationDetails));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.CreatedAt))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.CreatedAt));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.UpdatedAt));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.User))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.UserId));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.Animal))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.AnimalId));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.Shelter))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.ShelterId));
			}
			return [.. projectionFields];
		}

        
    }
}
