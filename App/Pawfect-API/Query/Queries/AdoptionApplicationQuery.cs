using MongoDB.Bson;
using MongoDB.Driver;

using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Authorization;
using Pawfect_API.DevTools;
using Pawfect_API.Exceptions;
using Pawfect_API.Services.AuthenticationServices;
using Pawfect_API.Services.Convention;
using Pawfect_API.Services.MongoServices;
using System.Security.Claims;


namespace Pawfect_API.Query.Queries
{
	public class AdoptionApplicationQuery : BaseQuery<Data.Entities.AdoptionApplication>
	{
        private readonly IConventionService _conventionService;
        private readonly IQueryFactory _queryFactory;

        public AdoptionApplicationQuery
		(
			MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
			ClaimsExtractor claimsExtractor,
            IAuthorizationContentResolver authorizationContentResolver,
			IConventionService conventionService,
			IQueryFactory queryFactory

        ) : base(mongoDbService, AuthorizationService, authorizationContentResolver, claimsExtractor)
        {
            _conventionService = conventionService;
            _queryFactory = queryFactory;
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
		public List<ApplicationStatus>? Status { get; set; }

		// Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
		public DateTime? CreatedFrom { get; set; }

		// Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
		public DateTime? CreatedTill { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public AdoptionApplicationQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Εφαρμόζει τα καθορισμένα φίλτρα στο ερώτημα
        // Έξοδος: FilterDefinition<AdoptionApplication> - ο ορισμός φίλτρου που θα χρησιμοποιηθεί στο ερώτημα
        public override async Task<FilterDefinition<Data.Entities.AdoptionApplication>> ApplyFilters()
		{
            FilterDefinitionBuilder<Data.Entities.AdoptionApplication> builder = Builders<Data.Entities.AdoptionApplication>.Filter;
            FilterDefinition<Data.Entities.AdoptionApplication> filter = builder.Empty;

			// Εφαρμόζει φίλτρο για τα IDs των αιτήσεων υιοθεσίας
			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.AdoptionApplication.Id), referenceIds.Where(id => id != ObjectId.Empty));
			}

            if (ExcludedIds != null && ExcludedIds.Any())
            {
                // Convert String IDs to ObjectId for comparison
                IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                // Ensure that only valid ObjectId values are passed in the filter
                filter &= builder.Nin(nameof(Data.Entities.AdoptionApplication.Id), referenceIds.Where(id => id != ObjectId.Empty));
            }

            // Εφαρμόζει φίλτρο για τα IDs των χρηστών
            if (UserIds != null && UserIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = UserIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.AdoptionApplication.UserId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των ζώων
			if (AnimalIds != null && AnimalIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = AnimalIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.AdoptionApplication.AnimalId), referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Εφαρμόζει φίλτρο για τα IDs των καταφυγίων
			if (ShelterIds != null && ShelterIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ShelterIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In(nameof(Data.Entities.AdoptionApplication.ShelterId), referenceIds.Where(id => id != ObjectId.Empty));
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

			if (!String.IsNullOrEmpty(base.Query))
			{
				UserQuery userQuery = _queryFactory.Query<UserQuery>();
				userQuery.Query = base.Query;
				userQuery.Offset = 0;
				userQuery.PageSize = base.PageSize;
				userQuery.Fields = userQuery.FieldNamesOf([nameof(Models.User.User.Id)]);
				userQuery = userQuery.Authorise(this._authorise);

				List<String> userIds = (await userQuery.CollectAsync())?.Select(user => user.Id).ToList() ?? [];
				if (userIds.Any())
                {
                    // Convert String IDs to ObjectId for comparison
                    IEnumerable<ObjectId> referenceIds = userIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

                    // Ensure that only valid ObjectId values are passed in the filter
                    filter &= builder.In(nameof(Data.Entities.AdoptionApplication.UserId), referenceIds.Where(id => id != ObjectId.Empty));
                }
            }

			return filter;
        }

        public override async Task<FilterDefinition<Data.Entities.AdoptionApplication>> ApplyAuthorization(
            FilterDefinition<Data.Entities.AdoptionApplication> filter)
        {
            if (_authorise.HasFlag(AuthorizationFlags.None)) return filter;

            if (_authorise.HasFlag(AuthorizationFlags.Permission))
                if (await _authorizationService.AuthorizeAsync(Permission.BrowseAdoptionApplications))
                    return filter;

            List<FilterDefinition<BsonDocument>> authorizationFilters = new List<FilterDefinition<BsonDocument>>();
            if (_authorise.HasFlag(AuthorizationFlags.Affiliation))
            {
                ClaimsPrincipal claimsPrincipal = _authorizationContentResolver.CurrentPrincipal();
                String userId = _claimsExtractor.CurrentUserId(claimsPrincipal);
                if (!_conventionService.IsValidId(userId)) throw new ForbiddenException("No authenticated user found");

                FilterDefinition<BsonDocument> affiliatedFilter = await _authorizationContentResolver.BuildAffiliatedFilterParams(typeof(Data.Entities.AdoptionApplication));
                authorizationFilters.Add(affiliatedFilter);
            }

            if (_authorise.HasFlag(AuthorizationFlags.Owner))
            {
                FilterDefinition<BsonDocument> ownedFilter = _authorizationContentResolver.BuildOwnedFilterParams(typeof(Data.Entities.AdoptionApplication));
                authorizationFilters.Add(ownedFilter);
            }

            if (authorizationFilters.Count == 0) return filter;

            FilterDefinition<BsonDocument> combinedAuthorizationFilter = Builders<BsonDocument>.Filter.Or(authorizationFilters);

            FilterDefinition<BsonDocument> combinedFinalBsonFilter = Builders<BsonDocument>.Filter.And(MongoHelper.ToBsonFilter<Data.Entities.AdoptionApplication>(filter), combinedAuthorizationFilter);

            return await Task.FromResult(MongoHelper.FromBsonFilter<Data.Entities.AdoptionApplication>(combinedFinalBsonFilter));
        }

        // Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
        // Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
        // Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
        public override List<String> FieldNamesOf(List<String> fields)
		{
            if (fields == null || !fields.Any()) return new List<String>();

            HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AdoptionApplicationDto στα ονόματα πεδίων AdoptionApplication
				projectionFields.Add(nameof(Data.Entities.AdoptionApplication.Id));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.Status))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.Status));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.ApplicationDetails))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.ApplicationDetails));
                if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.RejectReasson))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.RejectReasson));
                if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.CreatedAt))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.CreatedAt));
				if (item.Equals(nameof(Models.AdoptionApplication.AdoptionApplication.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.UpdatedAt));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.User))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.UserId));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.Animal))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.AnimalId));
				if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.Shelter))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.ShelterId));
                if (item.StartsWith(nameof(Models.AdoptionApplication.AdoptionApplication.AttachedFiles))) projectionFields.Add(nameof(Data.Entities.AdoptionApplication.AttachedFilesIds));
            }
            return [.. projectionFields];
		}

        
    }
}
