namespace Pawfect_API.Models.Lookups
{
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Pawfect_API.Data.Entities;
    using Pawfect_API.Data.Entities.EnumTypes;
    using Pawfect_API.DevTools;
    using Pawfect_API.Query;
    using Pawfect_API.Query.Queries;

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
        public List<String>? AnimalTypeIds { get; set; }

        // Λίστα από καταστάσεις υιοθεσίας για φιλτράρισμα
        public List<AdoptionStatus>? AdoptionStatuses { get; set; }
        public List<Gender>? Genders { get; set; }
        public Double? AgeFrom { get; set; }
        public Double? AgeTo { get; set; }

        // Ημερομηνία έναρξης για φιλτράρισμα (δημιουργήθηκε από)
        public DateTime? CreateFrom { get; set; }

        // Ημερομηνία λήξης για φιλτράρισμα (δημιουργήθηκε μέχρι)
        public DateTime? CreatedTill { get; set; }

        public Boolean? UseVectorSearch { get; set; }

        public Boolean? UseSemanticSearch { get; set; }

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
            if (this.AnimalTypeIds != null && this.AnimalTypeIds.Count != 0) animalQuery.AnimalTypeIds = this.AnimalTypeIds;
            if (this.AdoptionStatuses != null && this.AdoptionStatuses.Count != 0) animalQuery.AdoptionStatuses = this.AdoptionStatuses;
            if (this.Genders != null && this.Genders.Count != 0) animalQuery.Genders = this.Genders;
            if (this.AgeFrom.HasValue) animalQuery.AgeFrom = this.AgeFrom;
            if (this.AgeTo.HasValue) animalQuery.AgeTo = this.AgeTo;
            if (this.CreateFrom.HasValue) animalQuery.CreateFrom = this.CreateFrom;
            if (this.CreatedTill.HasValue) animalQuery.CreatedTill = this.CreatedTill;
            if (this.UseVectorSearch.HasValue) animalQuery.UseVectorSearch = this.UseVectorSearch;
            if (this.UseSemanticSearch.HasValue) animalQuery.UseSemanticSearch = this.UseSemanticSearch;
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