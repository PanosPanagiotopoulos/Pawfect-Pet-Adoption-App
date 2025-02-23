// FILE: Models/Lookups/AnimalTypeLookup.cs
namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	using Pawfect_Pet_Adoption_App_API.Data.Entities;
	using Pawfect_Pet_Adoption_App_API.Query.Queries;

	public class AnimalTypeLookup : Lookup
	{
		private AnimalTypeQuery _animalTypeQuery { get; set; }

		public AnimalTypeLookup(AnimalTypeQuery animalTypeQuery)
		{
			_animalTypeQuery = animalTypeQuery;
		}

		public AnimalTypeLookup() { }

		// Λίστα από IDs τύπων ζώων για φιλτράρισμα
		public List<String>? Ids { get; set; }

		// Ονομασία τύπων ζώων για φιλτράρισμα
		public String? Name { get; set; }

		/// <summary>
		/// Εμπλουτίζει το AnimalTypeQuery με τα φίλτρα και τις επιλογές του lookup.
		/// </summary>
		/// <returns>Το εμπλουτισμένο AnimalTypeQuery.</returns>
		public AnimalTypeQuery EnrichLookup(AnimalTypeQuery toEnrichQuery = null)
		{
			if (toEnrichQuery != null && _animalTypeQuery == null)
			{
				_animalTypeQuery = toEnrichQuery;
			}

			// Προσθέτει τα φίλτρα στο AnimalTypeQuery
			_animalTypeQuery.Ids = this.Ids;
			_animalTypeQuery.Name = this.Name;
			_animalTypeQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το AnimalTypeQuery
			_animalTypeQuery.PageSize = this.PageSize;
			_animalTypeQuery.Offset = this.Offset;
			_animalTypeQuery.SortDescending = this.SortDescending;
			_animalTypeQuery.Fields = _animalTypeQuery.FieldNamesOf(this.Fields.ToList());
			_animalTypeQuery.SortBy = this.SortBy;

			return _animalTypeQuery;
		}

		/// <summary>
		/// Επιστρέφει τον τύπο οντότητας του AnimalTypeLookup.
		/// </summary>
		/// <returns>Ο τύπος οντότητας του AnimalTypeLookup.</returns>
		public override Type GetEntityType() { return typeof(AnimalType); }
	}
}