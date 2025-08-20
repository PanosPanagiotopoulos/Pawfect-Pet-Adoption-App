using Pawfect_API.Query.Interfaces;
using Pawfect_API.Repositories.Implementations;
using Pawfect_API.Services.MongoServices;

namespace Pawfect_API.Query.Implementations
{
	public class FileRepository : BaseMongoRepository<Data.Entities.File>, IFileRepository
	{
		public FileRepository(MongoDbService dbService) : base(dbService) { }
	}
}
