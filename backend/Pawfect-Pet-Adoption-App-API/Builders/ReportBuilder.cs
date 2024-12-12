using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Report;

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

    // GET Response Dto Μοντέλα
}