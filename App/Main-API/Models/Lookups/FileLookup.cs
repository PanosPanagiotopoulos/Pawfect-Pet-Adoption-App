using MongoDB.Bson;
using MongoDB.Driver;
using Main_API.Data.Entities.EnumTypes;
using Main_API.DevTools;
using Main_API.Query;
using Main_API.Query.Queries;
using System.Reflection;

namespace Main_API.Models.Lookups
{
	public class FileLookup : Lookup
	{
		public List<String>? Ids { get; set; }

		public List<String>? ExcludedIds { get; set; }

		public List<String>? OwnerIds { get; set; }

		public List<FileSaveStatus>? FileSaveStatuses { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTill { get; set; }

        public FileQuery EnrichLookup(IQueryFactory queryFactory)
        {
            FileQuery fileQuery = queryFactory.Query<FileQuery>();

            // Προσθέτει τα φίλτρα στο FileQuery με if statements
            if (this.Ids != null && this.Ids.Count != 0) fileQuery.Ids = this.Ids;
            if (this.ExcludedIds != null && this.ExcludedIds.Count != 0) fileQuery.ExcludedIds = this.ExcludedIds;
            if (this.OwnerIds != null && this.OwnerIds.Count != 0) fileQuery.OwnerIds = this.OwnerIds;
            if (this.FileSaveStatuses != null && this.FileSaveStatuses.Count != 0) fileQuery.FileSaveStatuses = this.FileSaveStatuses;
            if (this.CreatedFrom.HasValue) fileQuery.CreatedFrom = this.CreatedFrom;
            if (this.CreatedTill.HasValue) fileQuery.CreatedTill = this.CreatedTill;
            if (!String.IsNullOrEmpty(this.Query)) fileQuery.Query = this.Query;

            fileQuery.Fields = fileQuery.FieldNamesOf([.. this.Fields]);

            base.EnrichCommon(fileQuery);

            return fileQuery;
        }

        public override async Task<FilterDefinition<BsonDocument>> ToFilters(IQueryFactory queryFactory)
        {
            FilterDefinition<Data.Entities.File> filters = await this.EnrichLookup(queryFactory).ApplyFilters();

            return MongoHelper.ToBsonFilter(filters);
        }

        // Επιστρέφει τον τύπο οντότητας του BreedLookup
        // Έξοδος: Ο τύπος οντότητας του BreedLookup
        public override Type GetEntityType() { return typeof(Data.Entities.File); }
	}
}
