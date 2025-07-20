using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Queries
{
	public interface IQuery {
        public int Offset { get; set; } 
        public int PageSize { get; set; } 
        public ICollection<String>? Fields { get; set; }
        public ICollection<String>? SortBy { get; set; }
        public Boolean? SortDescending { get; set; }
        public String? Query { get; set; } 
    }

	public abstract class BaseQuery<T> : IQuery where T : class
	{
		// Η συλλογή στην οποία αναφέρεται η ερώτηση
		protected IMongoCollection<T>? _collection { get; set; }

        protected readonly IAuthorizationService _authorizationService;

        protected readonly IAuthorizationContentResolver _authorizationContentResolver;

        protected readonly ClaimsExtractor _claimsExtractor;

        protected BaseQuery(
            MongoDbService mongoDbService,
            IAuthorizationService AuthorizationService,
            IAuthorizationContentResolver AuthorizationContentResolver,
            ClaimsExtractor claimsExtractor
        )
        {
            this._collection = mongoDbService.GetCollection<T>();
            this._authorizationService = AuthorizationService;
            this._authorizationContentResolver = AuthorizationContentResolver;
            this._claimsExtractor = claimsExtractor;
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
		public abstract Task<FilterDefinition<T>> ApplyAuthorization(FilterDefinition<T> filter);
        
		// Εφαρμόζει την προβολή για δυναμικά πεδία
        private IFindFluent<T, T> ApplyProjection(IFindFluent<T, T> finder)
		{
			if (Fields == null || Fields.Count == 0)
				return finder;

			Fields = FieldNamesOf(Fields.ToList());

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

			if (SortBy == null || SortBy.Count == 0)
			{
				return finder.Sort(builder.Ascending("CreatedAt").Ascending("_id"));
			}

			SortDefinition<T> sortDefinition = SortDescending.GetValueOrDefault()
				? builder.Descending(SortBy.First())
				: builder.Ascending(SortBy.First());

			foreach (String sortBy in SortBy.Skip(1))
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

            // Βήμα 2: Εφαρμογή Authorization στα δεδομένα που ζητούντε
            filter = await this.ApplyAuthorization(filter);

            IFindFluent<T, T> finder = _collection.Find(filter);

			// Βήμα 3: Εφαρμογή ταξινόμησης αν απαιτείται
			finder = this.ApplySorting(finder);

			// Βήμα 4: Εφαρμογή σελιδοποίησης
			finder = this.ApplyPagination(finder);

			// Βήμα 5: Εφαρμογή προβολής για δυναμικά πεδία
			finder = this.ApplyProjection(finder);

			// Εκτέλεση της ερώτησης και επιστροφή του αποτελέσματος
			return await finder.ToListAsync() ?? new List<T>();
		}
        public virtual async Task<long> CountAsync()
        {
            // Step 1: Apply filters
            FilterDefinition<T> filter = await this.ApplyFilters();

            // Step 2: Apply authorization
            filter = await this.ApplyAuthorization(filter);

            // Step 3: Count documents matching the filter
            return await _collection.CountDocumentsAsync(filter);
        }
    }
}