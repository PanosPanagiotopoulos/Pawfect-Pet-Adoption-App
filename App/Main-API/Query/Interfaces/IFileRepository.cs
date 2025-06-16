using Main_API.Repositories.Interfaces;

namespace Main_API.Query.Interfaces
{
	public interface IFileRepository: IMongoRepository<Data.Entities.File> { }
}
