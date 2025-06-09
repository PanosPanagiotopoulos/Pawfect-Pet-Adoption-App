using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.Models.User;

namespace Pawfect_Pet_Adoption_App_API.Models.File
{
	public class File
	{
		public String Id { get; set; }
		public String Filename { get; set; }
		public double? Size { get; set; }
		public User.User Owner { get; set; }
		public String MimeType { get; set; }
		public String FileType { get; set; }
		public FileSaveStatus? FileSaveStatus { get; set; }
		public String SourceUrl { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
