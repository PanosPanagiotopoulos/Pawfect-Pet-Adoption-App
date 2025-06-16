using Main_API.Query.Interfaces;
using Main_API.Repositories.Implementations;
using Main_API.Services.MongoServices;

namespace Main_API.Query.Implementations
{
	public class FileRepository : BaseMongoRepository<Data.Entities.File>, IFileRepository
	{
		public FileRepository(MongoDbService dbService) : base(dbService) { }
	}
}
