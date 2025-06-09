using System.ComponentModel.DataAnnotations;

namespace Pawfect_Pet_Adoption_App_API.Models.File
{
	public class TempMediaFile
	{
		[Required(ErrorMessage = "File is required.")]
		public IFormFile File { get; set; }
		[Required(ErrorMessage = "OwnerId is required.")]
		[RegularExpression(@"^[0-9a-fA-F]{24}$", ErrorMessage = "OwnerId must be a valid MongoDB ObjectId.")]
		public String OwnerId { get; set; }
	}
}
