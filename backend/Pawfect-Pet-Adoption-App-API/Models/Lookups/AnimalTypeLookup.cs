// FILE: Models/Lookups/AnimalTypeLookup.cs
namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

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
			animalTypeQuery.Ids = this.Ids;
			animalTypeQuery.Name = this.Name;
			animalTypeQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το AnimalTypeQuery
			animalTypeQuery.PageSize = this.PageSize;
			animalTypeQuery.Offset = this.Offset;
			animalTypeQuery.SortDescending = this.SortDescending;
			animalTypeQuery.Fields = animalTypeQuery.FieldNamesOf(this.Fields.ToList());
			animalTypeQuery.SortBy = this.SortBy;
			animalTypeQuery.ExcludedIds = this.ExcludedIds;

            return animalTypeQuery;
		}

		/// <summary>
		/// Επιστρέφει τον τύπο οντότητας του AnimalTypeLookup.
		/// </summary>
		/// <returns>Ο τύπος οντότητας του AnimalTypeLookup.</returns>
		public override Type GetEntityType() { return typeof(AnimalType); }
	}
}