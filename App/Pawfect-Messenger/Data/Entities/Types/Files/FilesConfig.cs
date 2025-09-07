namespace Pawfect_Messenger.Data.Entities.Types.Files
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
    }

    public class FileTypeConfig
    {
        public String FileType { get; set; }
        public List<String> MimeTypes { get; set; }
        public List<String> Extensions { get; set; }
    }
}
