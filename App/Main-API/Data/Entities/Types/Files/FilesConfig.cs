using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files
{
	public class FilesConfig
	{
		public int MaxRetryCount { get; set; }
		public int InitialRetryDelayMs { get; set; }
		public long MaxFileSizeBytes { get; set; }
		public int BatchSize { get; set; }
		public List<FileTypeConfig> AllowedFileTypes { get; set; }
	}

	public class FileTypeConfig
	{
		public String FileType { get; set; }
		public List<String> MimeTypes { get; set; }
		public List<String> Extensions { get; set; }
	}
}
