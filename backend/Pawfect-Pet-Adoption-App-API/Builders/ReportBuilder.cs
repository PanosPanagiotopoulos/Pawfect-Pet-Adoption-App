using AutoMapper;

using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Authorisation;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Query;
using Pawfect_Pet_Adoption_App_API.Services.UserServices;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
	public class AutoReportBuilder : Profile
	{
		// Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
		// Builder για Entity : Report
		public AutoReportBuilder()
		{
			// Mapping για το Entity : Report σε Report για χρήση του σε αντιγραφή αντικειμένων
			CreateMap<Report, Report>();

			// POST Request Dto Μοντέλα
			CreateMap<Report, ReportPersist>();
			CreateMap<ReportPersist, Report>();
		}
	}

	public class ReportBuilder : BaseBuilder<ReportDto, Report>
	{
        private readonly IQueryFactory _queryFactory;
        private readonly IBuilderFactory _builderFactory;
        public ReportBuilder(IQueryFactory queryFactory, IBuilderFactory builderFactory)
		{
            this._queryFactory = queryFactory;
            this._builderFactory = builderFactory;
        }

        public AuthorizationFlags _authorise = AuthorizationFlags.None;
       

        public ReportBuilder Authorise(AuthorizationFlags authorise) { this._authorise = authorise; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<ReportDto>> BuildDto(List<Report> entities, List<String> fields)
		{
			// Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
			(List<String> nativeFields, Dictionary<String, List<String>> foreignEntitiesFields) = ExtractBuildFields(fields);

			// Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
			Dictionary<String, List<UserDto>>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
				? (await CollectUsers(entities, foreignEntitiesFields[nameof(User)]))
				: null;

			List<ReportDto> result = new List<ReportDto>();
			foreach (Report e in entities)
			{
				ReportDto dto = new ReportDto();
				dto.Id = e.Id;
				if (nativeFields.Contains(nameof(Report.Type))) dto.Type = e.Type;
				if (nativeFields.Contains(nameof(Report.Reason))) dto.Reason = e.Reason;
				if (nativeFields.Contains(nameof(Report.Status))) dto.Status = e.Status;
				if (nativeFields.Contains(nameof(Report.CreatedAt))) dto.CreatedAt = e.CreatedAt;
				if (nativeFields.Contains(nameof(Report.UpdatedAt))) dto.UpdatedAt = e.UpdatedAt;
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.Reporter = userMap[e.Id][0];
				if (userMap != null && userMap.ContainsKey(e.Id)) dto.Reported = userMap[e.Id][1];

				result.Add(dto);
			}

			return await Task.FromResult(result);
		}

		private async Task<Dictionary<String, List<UserDto>>> CollectUsers(List<Report> reports, List<String> userFields)
		{
			// Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
			List<String> userIds = reports.SelectMany(x => new[] { x.ReporterId, x.ReportedId }).Distinct().ToList();

            UserLookup userLookup = new UserLookup();
            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            userLookup.Offset = 1;
            // Γενική τιμή για τη λήψη των dtos
            userLookup.PageSize = 1000;
            userLookup.Ids = userIds;
            userLookup.Fields = userFields;

            List<Data.Entities.User> users = await userLookup.EnrichLookup(_queryFactory).Authorise(this._authorise).CollectAsync();

            List<UserDto> userDtos = await _builderFactory.Builder<UserBuilder>().Authorise(this._authorise).BuildDto(users, userFields);

            if (userDtos == null || !userDtos.Any()) { return null; }

            // Δημιουργία ενός Dictionary με τον τύπο String ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<String, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

			// Ταίριασμα του προηγούμενου Dictionary με τα reports δημιουργώντας ένα Dictionary : [ ReportId -> UserId ] 
			return reports.ToDictionary(x => x.Id, x => new List<UserDto> { userDtoMap[x.ReporterId], userDtoMap[x.ReportedId] });
		}
	}
}