using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;

namespace Pawfect_Pet_Adoption_App_API.Services.Convention
{
	public interface IConventionService
	{
		Boolean IsValidId(String id);
		String ToExtention(String fileType);
		String ToFileType(String extension);
		(Boolean IsValid, String ErrorMessage) IsValidFile(IFormFile file, FilesConfig config);
	}
}
