using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Files;
using Main_API.Models.User;

namespace Main_API.Models.File
{
	public class File
	{
		public String Id { get; set; }
		public String Filename { get; set; }
		public Double? Size { get; set; }
		public User.User Owner { get; set; }
		public String MimeType { get; set; }
		public String FileType { get; set; }
		public FileSaveStatus? FileSaveStatus { get; set; }
		public String SourceUrl { get; set; }
		public DateTime? CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
