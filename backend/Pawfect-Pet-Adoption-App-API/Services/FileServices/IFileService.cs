using Pawfect_Pet_Adoption_App_API.Models.File;
using Pawfect_Pet_Adoption_App_API.Models.Lookups;

namespace Pawfect_Pet_Adoption_App_API.Services.FileServices
{
	public interface IFileService
	{
		Task<FileDto> Persist(FilePersist persist, List<String> fields);
		Task<List<FileDto>> Persist(List<FilePersist> persist, List<String> fields);
		Task<FileDto> SaveTemporarily(TempMediaFile tempMediaFile);
		Task<IEnumerable<FileDto>> SaveTemporarily(List<TempMediaFile> tempMediaFiles);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
