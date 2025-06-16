namespace Main_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Main_API.Data.Entities;
    using Main_API.Data.Entities.EnumTypes;
    using Main_API.DevTools;
    using Main_API.Query;
    using Main_API.Query.Queries;

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

            // Προσθέτει τα φίλτρα στο AnimalQuery με if statements
            if (this.Ids != null && this.Ids.Count != 0) animalQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) animalQuery.ExcludedIds = this.ExcludedIds;
            if (this.ShelterIds != null && this.ShelterIds.Count != 0) animalQuery.ShelterIds = this.ShelterIds;
            if (this.BreedIds != null && this.BreedIds.Count != 0) animalQuery.BreedIds = this.BreedIds;
            if (this.TypeIds != null && this.TypeIds.Count != 0) animalQuery.TypeIds = this.TypeIds;
            if (this.AdoptionStatuses != null && this.AdoptionStatuses.Count != 0) animalQuery.AdoptionStatuses = this.AdoptionStatuses;
            if (this.CreateFrom.HasValue) animalQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) animalQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) animalQuery.Query = this.Query;

            animalQuery.Fields = animalQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(animalQuery);

            return animalQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.Animal> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        /// <summary>
        /// Επιστρέφει τον τύπο οντότητας του AnimalLookup.
        /// </summary>
        /// <returns>Ο τύπος οντότητας του AnimalLookup.</returns>
        public override Type GetEntityType() { return typeof(Animal); }
    }
}