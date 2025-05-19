using Pawfect_Pet_Adoption_App_API.Query.Interfaces;
using Pawfect_Pet_Adoption_App_API.Repositories.Implementations;
using Pawfect_Pet_Adoption_App_API.Services.MongoServices;

namespace Pawfect_Pet_Adoption_App_API.Query.Implementations
{
	public class FileRepository : GeneralRepo<Data.Entities.File>, IFileRepository
	{
		public FileRepository(MongoDbService dbService, IHttpContextAccessor httpContextAccessor) : base(dbService, httpContextAccessor) { }
	}
}
