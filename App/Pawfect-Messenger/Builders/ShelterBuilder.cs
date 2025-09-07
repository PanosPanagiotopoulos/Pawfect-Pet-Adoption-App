using Pawfect_Messenger.Data.Entities.Types.Authorisation;
using Pawfect_Messenger.Models.Lookups;
using Pawfect_Messenger.Query;

namespace Pawfect_Messenger.Builders
{
	public class ShelterBuilder : BaseBuilder<Models.Shelter.Shelter, Data.Entities.Shelter>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ShelterBuilder
		(
		  IQueryFactory queryFactory,
          IBuilderFactory builderFactory

        )
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
        

        public ShelterBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }


        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<Models.Shelter.Shelter>> Build(List<Data.Entities.Shelter> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<String, Models.User.User>? userMap = foreignEntitiesFields.ContainsKey(nameof(Models.Shelter.Shelter.User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(Models.Shelter.Shelter.User)]))
				: null;

            List<Models.Shelter.Shelter> result = new List<Models.Shelter.Shelter>();
			foreach (Data.Entities.Shelter e in entities)
			{
                Models.Shelter.Shelter dto = new Models.Shelter.Shelter();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.ShelterName))) dto.ShelterName = e.ShelterName;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.Description))) dto.Description = e.Description;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.Website))) dto.Website = e.Website;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.SocialMedia))) dto.SocialMedia = e.SocialMedia;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.OperatingHours))) dto.OperatingHours = e.OperatingHours;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.VerificationStatus))) dto.VerificationStatus = e.VerificationStatus;
				if (nativeFields.Contains(nameof(Models.Shelter.Shelter.VerifiedBy))) dto.VerifiedBy = e.VerifiedById;

				if (userMap != null && userMap.ContainsKey(e.Id)) dto.User = userMap[e.Id];

                result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, Models.User.User>?> CollectUsers(List<Data.Entities.Shelter> shelters, List<String> userFields)
		{
            if (shelters.Count == 0 || userFields.Count == 0) return null;

            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<String> userIds = [.. shelters.Select(x => x.UserId).Distinct()];

            userFields.Add(String.Join('.', nameof(Models.Shelter.Shelter.User), nameof(Models.User.User.Id))); 

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
			return shelters.ToDictionary(x => x.Id, x => userDtoMap[x.UserId]);
		}
    }
}