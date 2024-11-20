using AutoMapper;
using Pawfect_Pet_Adoption_App_API.Data.Entities;
using Pawfect_Pet_Adoption_App_API.Models.Report;

namespace Pawfect_Pet_Adoption_App_API.Builders
{
    public class ReportBuilder : Profile
    {
        // Builders για μετατροπές object απο Entities σε Μοντέλα κάποιας χρήσης και αντίστροφα
        // Builder για Entity : Report
        public ReportBuilder()
        {
            // Mapping για το Entity : Report σε Report για χρήση του σε αντιγραφή αντικειμένων
            CreateMap<Report, Report>();

            // GET Response Dto Μοντέλα
            CreateMap<Report, ReportDto>();
            CreateMap<ReportDto, Report>();

            // POST Request Dto Μοντέλα
            CreateMap<Report, ReportPersist>();
            CreateMap<ReportPersist, Report>();
        }
    }
}