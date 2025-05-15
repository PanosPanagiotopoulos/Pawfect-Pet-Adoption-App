namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
    using Pawfect_Pet_Adoption_App_API.Data.Entities;
    using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
    using Pawfect_Pet_Adoption_App_API.Query;
    using Pawfect_Pet_Adoption_App_API.Query.Queries;

    public class AnimalLookup : Lookup
    {
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
        public AnimalQuery EnrichLookup(IQueryFactory queryFactory)
        {
            AnimalQuery animalQuery = queryFactory.Query<AnimalQuery>();

            // Προσθέτει τα φίλτρα στο AnimalQuery
            animalQuery.Ids = this.Ids;
            animalQuery.ShelterIds = this.ShelterIds;
            animalQuery.BreedIds = this.BreedIds;
            animalQuery.TypeIds = this.TypeIds;
            animalQuery.AdoptionStatuses = this.AdoptionStatuses;
            animalQuery.CreateFrom = this.CreateFrom;
            animalQuery.CreatedTill = this.CreatedTill;
            animalQuery.Query = this.Query;

            // Ορίζει επιπλέον επιλογές για το AnimalQuery
            animalQuery.PageSize = this.PageSize;
            animalQuery.Offset = this.Offset;
            animalQuery.SortDescending = this.SortDescending;
            animalQuery.Fields = animalQuery.FieldNamesOf(this.Fields.ToList());
            animalQuery.SortBy = this.SortBy;
            animalQuery.ExcludedIds = this.ExcludedIds;

            return animalQuery;
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AnimalLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AnimalLookup.</returns>
        public override Type GetEntityType() { return typeof(Animal); }
    }
}