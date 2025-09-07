using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;

namespace Pawfect_Messenger.Builders
{
	public class FileBuilder : BaseBuilder<Models.File.File, Data.Entities.File>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public FileBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        
        public FileBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.File.File>> Build(List<Data.Entities.File> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.User.User>? ownerMap = foreignEntitiesFields.ContainsKey(nameof(Models.File.File.Owner))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(Models.File.File.Owner)]))
				: null;

            List<Models.File.File> result = new List<Models.File.File>();
			foreach (Data.Entities.File e in entities)
			{
                Models.File.File dto = new Models.File.File();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.File.File.Filename))) dto.Filename = e.Filename;
				if (nativeFields.Contains(nameof(Models.File.File.FileType))) dto.FileType = e.FileType;
				if (nativeFields.Contains(nameof(Models.File.File.Size))) dto.Size = e.Size;
				if (nativeFields.Contains(nameof(Models.File.File.FileSaveStatus))) dto.FileSaveStatus = e.FileSaveStatus;
				if (nativeFields.Contains(nameof(Models.File.File.MimeType))) dto.MimeType = e.MimeType;
				if (nativeFields.Contains(nameof(Models.File.File.SourceUrl))) dto.SourceUrl = e.SourceUrl;
				if (nativeFields.Contains(nameof(Models.File.File.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Models.File.File.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (ownerMap != null && ownerMap.ContainsKey(e.Id)) dto.Owner = ownerMap[e.Id];


				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.User.User>?> CollectUsers(List<Data.Entities.File> files, List<String> userFields)
		{
            if (files.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. files.Select(x => x.OwnerId).Distinct()];

            UserLookup userLookup = new UserLookup();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 0;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<Models.User.User> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).Build(users, userFields);

            if (userDtos == null || userDtos.Count == 0) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, Models.User.User> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα shelters δημιουργώντας ένα Dictionary : [ ShelterId -> UserId ] 
			return files.ToDictionary(x => x.Id, x => userDtoMap[x.OwnerId]);
		}
	}
}