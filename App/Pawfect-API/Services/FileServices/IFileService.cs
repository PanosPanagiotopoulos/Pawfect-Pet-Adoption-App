using Pawfect_API.Models.File;
using Pawfect_API.Models.Lookups;

namespace Pawfect_API.Services.FileServices
{
	public interface IFileService
	{
		Task<Models.File.File> Persist(FilePersist persist, List<String> fields, Boolean auth = true);
		Task<List<Models.File.File>> Persist(List<FilePersist> persist, List<String> fields, Boolean auth = true);
		Task<Models.File.FilePersist> SaveTemporarily(IFormFile file);
		Task<List<Models.File.FilePersist>> SaveTemporarily(List<IFormFile> files);
		Task Delete(String id);
		Task Delete(List<String> ids);
	}
}
