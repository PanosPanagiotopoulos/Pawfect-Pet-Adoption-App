using Pawfect_Pet_Adoption_App_API.Repositories.Interfaces;

namespace Pawfect_Pet_Adoption_App_API.Query.Interfaces
{
	public interface IFileRepository: IMongoRepository<Data.Entities.File> { }
}
