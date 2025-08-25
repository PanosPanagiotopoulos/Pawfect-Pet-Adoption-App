
namespace Pawfect_API.Data.Entities.Types.Files
{
	public class FilesConfig
	{
		public int MaxRetryCount { get; set; }
		public int InitialRetryDelayMs { get; set; }
		public long MaxFileSizeBytes { get; set; }
		public int BatchSize { get; set; }
        public int PrivateFilesExpiryTime { get; set; }
        public List<FileTypeConfig> AllowedFileTypes { get; set; }
        public List<String> PrivateFileTypes { get; set; }
        public ExcelExtractorConfig ExcelExtractorConfig { get; set; }
    }

	public class FileTypeConfig
	{
		public String FileType { get; set; }
		public List<String> MimeTypes { get; set; }
		public List<String> Extensions { get; set; }
	}

    public class ExcelExtractorConfig
    {
        public List<String> Headers { get; set; } 

        public Dictionary<String, List<String>> StaticDropdowns { get; set; }

        public Dictionary<String, DynamicDropdownConfig> DynamicDropdowns { get; set; } 

        public String EditableRange { get; set; } = "A2:I1000";

        public Boolean Protected { get; set; } = true;

        public String ProtectionPassword { get; set; } = "readonly";

        public ReferenceSheetConfig ReferenceSheet { get; set; } 

        public int MaxRows { get; set; } = 1000;

        public Boolean AllowSheetExpansion { get; set; } = false;
    }

    public class DynamicDropdownConfig
    {
        public String SourceSheet { get; set; } = "ReferenceData";

        public String SourceColumn { get; set; } = "A";

        public String ReferenceListKey { get; set; } // e.g., "AnimalTypes", "Breeds"
    }

    public class ReferenceSheetConfig
    {
        public String Name { get; set; } = "ReferenceData";

        public Boolean Hidden { get; set; } = true;

        public Dictionary<String, String> Columns { get; set; } = new()
        {
            { "A", "AnimalTypeName" },
            { "B", "BreedName" }
        };
    }

}
