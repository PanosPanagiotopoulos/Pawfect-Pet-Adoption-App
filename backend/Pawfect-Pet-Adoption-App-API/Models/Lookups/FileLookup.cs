using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Query.Queries;

namespace Pawfect_Pet_Adoption_App_API.Models.Lookups
{
	public class FileLookup : Lookup
	{
        private FileQuery _fileQuery { get; set; }

		// Constructor για την κλάση BreedLookup
		// Είσοδος: breedQuery - μια έκδοση της κλάσης BreedQuery
		public FileLookup(FileQuery fileQuery)
		{
			_fileQuery = fileQuery;
		}

		public FileLookup() { }

		public List<String>? Ids { get; set; }

		public List<String>? ExcludedIds { get; set; }

		public List<String>? OwnerIds { get; set; }

		public List<FileSaveStatus>? FileSaveStatuses { get; set; }


		public FileQuery EnrichLookup(FileQuery? toEnrichQuery = null)
		{
			if (_fileQuery == null && toEnrichQuery != null)
			{
				_fileQuery = toEnrichQuery;
			}

			// Προσθέτει τα φίλτρα στο BreedQuery
			_fileQuery.Ids = this.Ids;
			_fileQuery.OwnerIds = this.OwnerIds;
			_fileQuery.FileSaveStatuses = this.FileSaveStatuses;
			_fileQuery.Query = this.Query;

			// Ορίζει επιπλέον επιλογές για το BreedQuery
			_fileQuery.PageSize = this.PageSize;
			_fileQuery.Offset = this.Offset;
			_fileQuery.SortDescending = this.SortDescending;
			_fileQuery.Fields = _fileQuery.FieldNamesOf(this.Fields.ToList());
			_fileQuery.SortBy = this.SortBy;
			_fileQuery.ExcludedIds = this.ExcludedIds;

			return _fileQuery;
		}

		// Επιστρέφει τον τύπο οντότητας του BreedLookup
		// Έξοδος: Ο τύπος οντότητας του BreedLookup
		public override Type GetEntityType() { return typeof(Data.Entities.File); }
	}
}
