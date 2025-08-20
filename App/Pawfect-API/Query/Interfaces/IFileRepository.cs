using Pawfect_API.Repositories.Interfaces;

namespace Pawfect_API.Query.Interfaces
{
	public interface IFileRepository: IMongoRepository<Data.Entities.File> { }
}
