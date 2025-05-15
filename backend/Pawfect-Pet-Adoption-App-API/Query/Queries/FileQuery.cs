using MongoDB.Bson;
using MongoDB.Driver;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.DevTools;
using Pawfect_Pet_Adoption_App_API.Models.AnimalType;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Queries
{
	public class FileQuery : BaseQuery<Data.Entities.File>
	{
		// Είσοδος: mongoDbService - μια έκδοση της κλάσης MongoDbService
		public FileQuery(MongoDbService mongoDbService)
		{
			base._collection = mongoDbService.GetCollection<Data.Entities.File>();
		}

		// Λίστα από IDs τύπων ζώων για φιλτράρισμα
		public List<String>? Ids { get; set; }

		public List<String>? ExcludedIds { get; set; }

		public List<String>? OwnerIds { get; set; }

		public List<FileSaveStatus>? FileSaveStatuses { get; set; }

		public String? Name { get; set; }

        private AuthorizationFlags _authorise = AuthorizationFlags.None;

        public FileQuery Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        public override Task<FilterDefinition<Data.Entities.File>> ApplyFilters()
		{
			FilterDefinitionBuilder<Data.Entities.File> builder = Builders<Data.Entities.File>.Filter;
			FilterDefinition<Data.Entities.File> filter = builder.Empty;

			if (Ids != null && Ids.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = Ids.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.In("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

			if (ExcludedIds != null && ExcludedIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = ExcludedIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin("Id", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Owned By Ids
			if (OwnerIds != null && OwnerIds.Any())
			{
				// Convert String IDs to ObjectId for comparison
				IEnumerable<ObjectId> referenceIds = OwnerIds.Select(id => ObjectId.TryParse(id, out ObjectId objectId) ? objectId : ObjectId.Empty);

				// Ensure that only valid ObjectId values are passed in the filter
				filter &= builder.Nin("OwnerId", referenceIds.Where(id => id != ObjectId.Empty));
			}

			// Owned By Ids
			if (FileSaveStatuses != null && FileSaveStatuses.Any())
			{
				filter &= builder.In(file => file.FileSaveStatus, FileSaveStatuses);
			}

			// Εφαρμόζει φίλτρο για fuzzy search μέσω indexing στη Mongo σε πεδίο : Filename
			if (!String.IsNullOrEmpty(Query))
			{
				filter &= builder.Text(Query);
			}

			return Task.FromResult(filter);
		}

		// Επιστρέφει τα ονόματα πεδίων που θα προβληθούν στο αποτέλεσμα του ερωτήματος
		// Είσοδος: fields - μια λίστα με τα ονόματα των πεδίων που θα προβληθούν
		// Έξοδος: List<String> - τα ονόματα των πεδίων που θα προβληθούν
		public override List<String> FieldNamesOf(List<String> fields)
		{
			if (fields == null || !fields.Any() || fields.Contains("*")) return fields = EntityHelper.GetAllPropertyNames(typeof(AnimalTypeDto)).ToList();

			HashSet<String> projectionFields = new HashSet<String>();
			foreach (String item in fields)
			{
				// Αντιστοιχίζει τα ονόματα πεδίων AnimalTypeDto στα ονόματα πεδίων AnimalType
				projectionFields.Add(nameof(Data.Entities.File.Id));
				if (item.Equals(nameof(Models.File.FileDto.Filename))) projectionFields.Add(nameof(Data.Entities.File.Filename));
				if (item.Equals(nameof(Models.File.FileDto.FileType))) projectionFields.Add(nameof(Data.Entities.File.FileType));
				if (item.Equals(nameof(Models.File.FileDto.MimeType))) projectionFields.Add(nameof(Data.Entities.File.MimeType));
				if (item.Equals(nameof(Models.File.FileDto.Size))) projectionFields.Add(nameof(Data.Entities.File.Size));
				if (item.Equals(nameof(Models.File.FileDto.FileSaveStatus))) projectionFields.Add(nameof(Data.Entities.File.FileSaveStatus));
				if (item.Equals(nameof(Models.File.FileDto.SourceUrl))) projectionFields.Add(nameof(Data.Entities.File.SourceUrl));
				if (item.Equals(nameof(Models.File.FileDto.CreatedAt))) projectionFields.Add(nameof(Data.Entities.File.CreatedAt));
				if (item.Equals(nameof(Models.File.FileDto.UpdatedAt))) projectionFields.Add(nameof(Data.Entities.File.UpdatedAt));
				if (item.StartsWith(nameof(Models.File.FileDto.Owner))) projectionFields.Add(nameof(Data.Entities.File.OwnerId));

			}

			return projectionFields.ToList();
		}
	}
}
