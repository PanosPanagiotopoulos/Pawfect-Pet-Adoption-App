// FILE: Models/Lookups/AnimalLookup.cs
namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class AnimalLookup : Lookup
    {
        private AnimalQuery _animalQuery { get; set; }

        public AnimalLookup(AnimalQuery animalQuery)
        {
            _animalQuery = animalQuery;
        }

        public AnimalLookup() { }

        // Λίστα από IDs ζώων για φιλτράρισμα
        public List<String>? Ids { get; set; }

        public List<String>? ExcludedIds { get; set; }


        // Λίστα από IDs καταφυγίων για φιλτράρισμα
        public List<String>? ShelterIds { get; set; }

        // Λίστα από IDs φυλών για φιλτράρισμα
        public List<String>? BreedIds { get; set; }

        // Λίστα από IDs τύπων για φιλτράρισμα
        public List<String>? TypeIds { get; set; }

        // Λίστα από καταστάσεις υιοθεσίας για φιλτράρισμα
        public List<AdoptionStatus>? AdoptionStatuses { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        /// <summary>
        /// Εμπλουτίζει το AnimalQuery με τα φίλτρα και τις επιλογές του lookup.
        /// </summary>
        /// <returns>Το εμπλουτισμένο AnimalQuery.</returns>
        public AnimalQuery EnrichLookup(AnimalQuery? toEnrichQuery = null)
        {
            if (toEnrichQuery != null && _animalQuery == null)
            {
                _animalQuery = toEnrichQuery;
            }

            // Προσθέτει τα φίλτρα στο AnimalQuery
            _animalQuery.Ids = this.Ids;
            _animalQuery.ShelterIds = this.ShelterIds;
            _animalQuery.BreedIds = this.BreedIds;
            _animalQuery.TypeIds = this.TypeIds;
            _animalQuery.AdoptionStatuses = this.AdoptionStatuses;
            _animalQuery.CreateFrom = this.CreateFrom;
            _animalQuery.CreatedTill = this.CreatedTill;
            _animalQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το AnimalQuery
            _animalQuery.PageSize = this.PageSize;
            _animalQuery.Offset = this.Offset;
            _animalQuery.SortDescending = this.SortDescending;
            _animalQuery.Fields = _animalQuery.FieldNamesOf(this.Fields.ToList());
            _animalQuery.SortBy = this.SortBy;
            _animalQuery.ExcludedIds = this.ExcludedIds;

            return _animalQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AnimalLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AnimalLookup.</returns>
        public override Type GetEntityType() { return typeof(Animal); }
    }
}