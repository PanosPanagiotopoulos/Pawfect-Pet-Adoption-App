using Main_API.Data.Entities.EnumTypes;
using Main_API.Data.Entities.Types.Files;

namespace Main_API.Models.File
{
	public class FilePersist
	{
		public String Id { get; set; }
		public String Filename { get; set; }
		public Double Size { get; set; }
		public String OwnerId { get; set; }
		public String MimeType { get; set; }
		public String FileType { get; set; }
		public FileSaveStatus? FileSaveStatus { get; set; }
		public String SourceUrl { get; set; }
        public String AwsKey { get; set; }
    }
}
