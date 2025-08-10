using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Main_API.Services.AuthenticationServices;
using Main_API.Services.MongoServices;
using System.Text.RegularExpressions;
using MongoDB.Bson;

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
        protected IFindFluent<T, T> ApplyProjection(IFindFluent<T, T> finder)
		{
			if (Fields == null || Fields.Count == 0)
				return finder;

			ProjectionDefinition<T> projection = Builders<T>.Projection.Include(Fields.First());
			foreach (String field in Fields.Skip(1))
			{
				projection = projection.Include(field);
			}

			return finder.Project<T>(projection);
		}

        protected List<BsonDocument> ApplyProjection(List<BsonDocument> pipeline)
        {
            if (Fields == null || Fields.Count == 0)
                return pipeline;

            BsonDocument projectStage = new BsonDocument();

            this.Fields = this.FieldNamesOf(this.Fields.ToList());

            // Include requested fields
            foreach (String field in this.Fields) projectStage[field] = 1;

            // Add the $project stage to pipeline
            pipeline.Add(new BsonDocument("$project", projectStage));

            return pipeline;
        }

        // Εφαρμόζει την ταξινόμηση στην ερώτηση
        protected IFindFluent<T, T> ApplySorting(IFindFluent<T, T> finder)
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

		protected List<BsonDocument> ApplySorting(List<BsonDocument> pipeline)
        {
            BsonDocument sortStage = new BsonDocument();

            if (this.SortBy == null || this.SortBy.Count == 0)
            {
                // Default sort: CreatedAt ascending, then _id ascending
                sortStage["CreatedAt"] = 1;
                sortStage["_id"] = 1;
            }
            else
            {
                Boolean descending = this.SortDescending.GetValueOrDefault(false);
                int sortDirection = descending ? -1 : 1;

                // Add all sort fields with the same direction
                foreach (String sortField in this.SortBy)
                {
                    sortStage[sortField] = sortDirection;
                }

                // Always add _id as final tie-breaker (ascending)
                if (!sortStage.Contains("_id"))
                {
                    sortStage["_id"] = 1;
                }
            }

            // Add the $sort stage to pipeline
            pipeline.Add(new BsonDocument("$sort", sortStage));

            return pipeline;
        }

        // Εφαρμόζει την σελιδοποίηση στην ερώτηση
        protected IFindFluent<T, T> ApplyPagination(IFindFluent<T, T> finder)
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
        protected List<BsonDocument> ApplyPagination(List<BsonDocument> pipeline)
        {
            int skipCount = (Math.Max(this.Offset - 1, 0)) * this.PageSize;
            pipeline.Add(new BsonDocument("$skip", skipCount));

            pipeline.Add(new BsonDocument("$limit", Math.Max(this.PageSize, 1)));

            return pipeline;
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

        #region Helper
		protected String CleanQuery() => Regex.Replace(Regex.Replace(Query ?? "", @"[^\w\s]", "").ToLowerInvariant().Trim(), @"\s+", " ");
        #endregion
    }
}