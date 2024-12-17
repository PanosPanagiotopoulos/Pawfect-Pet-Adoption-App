using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;
using Pawfect_Pet_Adoption_App_API.Models.Report;
using Pawfect_Pet_Adoption_App_API.Models.User;
using Pawfect_Pet_Adoption_App_API.Services;

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
        private readonly UserLookup _userLookup;
        private readonly IUserService _userService;

        public ReportBuilder(UserLookup userLookup, IUserService userService)
        {
            _userLookup = userLookup;
            _userService = userService;
        }

        // Ορίστε τις παραμέτρους αναζήτησης για τον κατασκευαστή
        public override BaseBuilder<ReportDto, Report> SetLookup(Lookup lookup) { base.LookupParams = lookup; return this; }

        // Κατασκευή των μοντέλων Dto βάσει των παρεχόμενων entities και πεδίων
        public override async Task<List<ReportDto>> BuildDto(List<Report> entities, List<string> fields)
        {
            // Εξαγωγή των αρχικών πεδίων και των πεδίων ξένων entities από τα παρεχόμενα πεδία
            (List<string> nativeFields, Dictionary<string, List<string>> foreignEntitiesFields) = ExtractBuildFields(fields);

            // Δημιουργία ενός Dictionary με τον τύπο string ως κλειδί και το "Dto model" ως τιμή για κάθε ξένο entity που ζητείται να επιστραφούν τα δεδομένα για αυτό
            Dictionary<string, List<UserDto>>? userMap = foreignEntitiesFields.ContainsKey(nameof(User))
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

        private async Task<Dictionary<string, List<UserDto>>> CollectUsers(List<Report> reports, List<string> userFields)
        {
            // Λήψη των αναγνωριστικών των ξένων κλειδιών για να γίνει ερώτημα στα επιπλέον entities
            List<string> userIds = reports.SelectMany(x => new[] { x.ReporterId, x.ReportedId }).Distinct().ToList();

            // Προσθήκη βασικών παραμέτρων αναζήτησης για το ερώτημα μέσω των αναγνωριστικών
            _userLookup.Offset = LookupParams.Offset;
            // Γενική τιμή για τη λήψη των dtos
            _userLookup.PageSize = LookupParams.PageSize;
            _userLookup.SortDescending = LookupParams.SortDescending;
            _userLookup.Query = null;
            _userLookup.Ids = userIds;
            _userLookup.Fields = userFields;

            // Κατασκευή των dtos
            List<UserDto> userDtos = (await _userService.QueryUsersAsync(_userLookup)).ToList();

            // Δημιουργία ενός Dictionary με τον τύπο string ως κλειδί και το "Dto model" ως τιμή : [ UserId -> UserDto ]
            Dictionary<string, UserDto> userDtoMap = userDtos.ToDictionary(x => x.Id);

            // Ταίριασμα του προηγούμενου Dictionary με τα reports δημιουργώντας ένα Dictionary : [ ReportId -> UserId ] 
            return reports.ToDictionary(x => x.Id, x => new List<UserDto> { userDtoMap[x.ReporterId], userDtoMap[x.ReportedId] });
        }
    }
}