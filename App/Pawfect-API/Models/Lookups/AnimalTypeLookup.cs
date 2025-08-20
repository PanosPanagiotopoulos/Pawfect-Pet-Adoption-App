// FILE: Models/Lookups/AnimalTypeLookup.cs
namespace Pawfect_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_API.Data.Entities;
    using Pawfect_API.DevTools;
    using Pawfect_API.Query;
    using Pawfect_API.Query.Queries;

	public class AnimalTypeLookup : Lookup
	{
		// Λίστα από IDs τύπων ζώων για φιλτράρισμα
		public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Ονομασία τύπων ζώων για φιλτράρισμα
        public String? Name { get; set; }

		/// <summary>
		/// Εμπλουτίζει το AnimalTypeQuery με τα φίλτρα και τις επιλογές του lookup.
		/// </summary>
		/// <returns>Το εμπλουτισμένο AnimalTypeQuery.</returns>
		public AnimalTypeQuery EnrichLookup(IQueryFactory queryFactory)
		{
			AnimalTypeQuery animalTypeQuery = queryFactory.Query<AnimalTypeQuery>();

			// Προσθέτει τα φίλτρα στο AnimalTypeQuery
			if (this.Ids != null && this.Ids.Count != 0) animalTypeQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) animalTypeQuery.ExcludedIds = this.ExcludedIds;
            if (!String.IsNullOrEmpty(this.Query))  animalTypeQuery.Query = this.Query;

            animalTypeQuery.Fields = animalTypeQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(animalTypeQuery);

            return animalTypeQuery;
		}

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.AnimalType> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AnimalTypeLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AnimalTypeLookup.</returns>
        public override Type GetEntityType() { return typeof(AnimalType); }
	}
}