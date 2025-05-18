using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices
{
	public interface IFileService
	{
		Task<Models.File.File> Persist(FilePersist persist, List<String> fields);
		Task<List<Models.File.File>> Persist(List<FilePersist> persist, List<String> fields);
		Task<Models.File.File> SaveTemporarily(TempMediaFile tempMediaFile);
		Task<IEnumerable<Models.File.File>> SaveTemporarily(List<TempMediaFile> tempMediaFiles);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
