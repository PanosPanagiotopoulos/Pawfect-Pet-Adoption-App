using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public abstract class BaseQuery<T> where T : class
	{
		// Η συλλογή στην οποία αναφέρεται η ερώτηση
		protected IMongoCollection<T>? _collection { get; set; }
		public int Offset { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public ICollection<String>? Fields { get; set; }
		public ICollection<String>? SortBy { get; set; }
		public Boolean? SortDescending { get; set; } = false;
		public String? Query { get; set; } = null;

		// Εφαρμόζει τα φίλτρα στην ερώτηση
		protected abstract Task<FilterDefinition<T>> ApplyFilters();

		// Επιστρέφει τα ονόματα των πεδίων που περιέχονται στη λίστα fields
		public abstract List<String> FieldNamesOf(List<String> fields);

		// Εφαρμόζει την προβολή για δυναμικά πεδία
		private IFindFluent<T, T> ApplyProjection(IFindFluent<T, T> finder)
		{
			if (Fields == null || !Fields.Any())
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

		// Εφαρμόζει την σελιδοποίηση στην ερώτηση
		private IFindFluent<T, T> ApplyAuthorisation(IFindFluent<T, T> finder)
		{
			// TOOD : Κατασκευή λογικής authorisation στο querying
			return finder;
		}

		// Συλλέγει τα αποτελέσματα της ερώτησης
		public virtual async Task<List<T>> CollectAsync()
		{
			// Βήμα 1: Εφαρμογή φίλτρων στην ερώτηση
			FilterDefinition<T> filter = await ApplyFilters();

			// Αρχικοποίηση της λειτουργίας αναζήτησης
			IFindFluent<T, T> finder = _collection.Find(filter);

			// Βήμα 2: Εφαρμογή ταξινόμησης αν απαιτείται
			finder = ApplySorting(finder);

			// Βήμα 3: Εφαρμογή σελιδοποίησης
			finder = ApplyPagination(finder);

			// Βήμα 4: Εφαρμογή προβολής για δυναμικά πεδία
			finder = ApplyProjection(finder);
			
			// Βήμα 5: Εφαρμογή authorisation στα δεδομένα που ζητούντε
			finder = ApplyAuthorisation(finder);

			// Εκτέλεση της ερώτησης και επιστροφή του αποτελέσματος
			return (await finder.ToListAsync()) ?? new List<T>();
		}
	}
}