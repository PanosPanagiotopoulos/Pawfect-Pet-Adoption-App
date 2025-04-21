using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;

namespace Pawfect_Pet_Adoption_App_API.Models.File
{
	public class FilePersist
	{
		public String Id { get; set; }
		public String Filename { get; set; }
		public double Size { get; set; }
		public String OwnerId { get; set; }
		public String MimeType { get; set; }
		public String FileType { get; set; }
		public FileSaveStatus? FileSaveStatus { get; set; }
		public String SourceUrl { get; set; }
	}
}
