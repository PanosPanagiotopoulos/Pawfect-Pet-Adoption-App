using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Pawfect_Pet_Adoption_App_API.Services.AuthenticationServices;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public interface IQuery { }

	public abstract class BaseQuery<T> : IQuery where T : class
	{
		// Η συλλογή στην οποία αναφέρεται η ερώτηση
		protected IMongoCollection<T>? _collection { get; set; }

        protected readonly IAuthorisationService _authorisationService;

        protected readonly IAuthorisationContentResolver _authorisationContentResolver;

        protected readonly ClaimsExtractor _claimsExtractor;

        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseQuery(
            MongoDbService mongoDbService,
            IAuthorisationService authorisationService,
            IAuthorisationContentResolver authorisationContentResolver,
            ClaimsExtractor claimsExtractor,
            IHttpContextAccessor httpContextAccessor
        )
        {
            this._collection = mongoDbService.GetCollection<T>();
            this._authorisationService = authorisationService;
            this._authorisationContentResolver = authorisationContentResolver;
            this._claimsExtractor = claimsExtractor;
            this._httpContextAccessor = httpContextAccessor;
        }


        // Base Query fields
        public int Offset { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public ICollection<String>? Fields { get; set; }
		public ICollection<String>? SortBy { get; set; }
		public Boolean? SortDescending { get; set; } = false;
		public String? Query { get; set; } = null;

		// Εφαρμόζει τα φίλτρα στην ερώτηση
		public abstract Task<FilterDefinition<T>> ApplyFilters();

		// Επιστρέφει τα ονόματα των πεδίων που περιέχονται στη λίστα fields
		public abstract List<String> FieldNamesOf(List<String> fields);

		// Εφαρμόζει την σελιδοποίηση στην ερώτηση
		public abstract Task<FilterDefinition<T>> ApplyAuthorisation(FilterDefinition<T> filter);
        
		// Εφαρμόζει την προβολή για δυναμικά πεδία
        private IFindFluent<T, T> ApplyProjection(IFindFluent<T, T> finder)
		{
			if (Fields == null || Fields.Count == 0)
			{
				return finder.Project<T>(Builders<T>.Projection.Exclude("_id"));
			}

			ProjectionDefinition<T> projection = Builders<T>.Projection.Include(Fields.First());
			foreach (String field in Fields.Skip(1))
			{
				projection = projection.Include(field);
			}

			return finder.Project<T>(projection);
		}

		// Εφαρμόζει την ταξινόμηση στην ερώτηση
		private IFindFluent<T, T> ApplySorting(IFindFluent<T, T> finder)
		{
			SortDefinitionBuilder<T> builder = Builders<T>.Sort;

			if (SortBy == null || !SortBy.Any())
			{
				return finder.Sort(builder.Ascending("CreatedAt").Ascending("_id"));
			}

			SortDefinition<T> sortDefinition = SortDescending.GetValueOrDefault()
				? builder.Descending(SortBy.First())
				: builder.Ascending(SortBy.First());

			foreach (var sortBy in SortBy.Skip(1))
			{
				sortDefinition = SortDescending.GetValueOrDefault()
					? builder.Descending(sortBy)
					: builder.Ascending(sortBy);
			}

			sortDefinition = sortDefinition.Ascending("_id");

			return finder.Sort(sortDefinition);
		}

		// Εφαρμόζει την σελιδοποίηση στην ερώτηση
		private IFindFluent<T, T> ApplyPagination(IFindFluent<T, T> finder)
		{
			if (Offset > 0)
			{
				finder = finder.Skip(( Math.Max(Offset - 1, 0) ) * PageSize);
			}

			if (PageSize > 0)
			{
				finder = finder.Limit(PageSize);
			}

			return finder;
		}

		// Συλλέγει τα αποτελέσματα της ερώτησης
		public virtual async Task<List<T>> CollectAsync()
		{
			// Βήμα 1: Εφαρμογή φίλτρων στην ερώτηση
			FilterDefinition<T> filter = await this.ApplyFilters();

            // Βήμα 2: Εφαρμογή authorisation στα δεδομένα που ζητούντε
            filter = await this.ApplyAuthorisation(filter);

            // Initialize the find operation with session if available
            IClientSessionHandle session = this.Session();
            IFindFluent<T, T> finder = session != null
                ? _collection.Find(session, filter)
                : _collection.Find(filter);

			// Βήμα 3: Εφαρμογή ταξινόμησης αν απαιτείται
			finder = this.ApplySorting(finder);

			// Βήμα 4: Εφαρμογή σελιδοποίησης
			finder = this.ApplyPagination(finder);

			// Βήμα 5: Εφαρμογή προβολής για δυναμικά πεδία
			finder = this.ApplyProjection(finder);

			// Εκτέλεση της ερώτησης και επιστροφή του αποτελέσματος
			return await finder.ToListAsync() ?? new List<T>();
		}

        private IClientSessionHandle Session() =>
            _httpContextAccessor.HttpContext?.Items["MongoSession"] as IClientSessionHandle;
    }
}