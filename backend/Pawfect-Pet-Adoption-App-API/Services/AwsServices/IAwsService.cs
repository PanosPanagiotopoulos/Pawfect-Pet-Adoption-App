namespace Pawfect_Pet_Adoption_App_API.Services.AwsServices
{
	public interface IAwsService
	{
		Task<String> UploadAsync(IFormFile file, String key);
		Task<String> GetAsync(String key);
		Task<Dictionary<String, Boolean>> DeleteAsync(String key);
		Task<Dictionary<String, Boolean>> DeleteAsync(List<String> key);
		String ConstructAwsKey(params String[] keyParts);
	}
}
