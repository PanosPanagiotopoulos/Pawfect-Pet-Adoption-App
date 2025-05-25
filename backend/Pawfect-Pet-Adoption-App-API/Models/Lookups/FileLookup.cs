using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Query.Queries;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
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

            // Προσθέτει τα φίλτρα στο BreedQuery
            fileQuery.Ids = this.Ids;
			fileQuery.OwnerIds = this.OwnerIds;
			fileQuery.FileSaveStatuses = this.FileSaveStatuses;
			fileQuery.CreatedFrom = this.CreatedFrom;
			fileQuery.CreatedTill = this.CreatedTill;
			fileQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το BreedQuery
			fileQuery.PageSize = this.PageSize;
			fileQuery.Offset = this.Offset;
			fileQuery.SortDescending = this.SortDescending;
			fileQuery.Fields = fileQuery.FieldNamesOf([.. this.Fields]);
			fileQuery.SortBy = this.SortBy;
			fileQuery.ExcludedIds = this.ExcludedIds;

			return fileQuery;
		}

        // Επιστρέφει τον τύπο οντότητας του BreedLookup
        // Έξοδος: Ο τύπος οντότητας του BreedLookup
        public override Type GetEntityType() { return typeof(Data.Entities.File); }
	}
}
