using Pawfect_Pet_Adoption_App_API.Models;
using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;
using Pawfect_Pet_Adoption_App_API.Services;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Implementations
{
    public class ReportRepository : GeneralRepo<Report>, IReportRepository
    {
        public ReportRepository(MongoDbService dbService) : base(dbService) { }
    }
}
