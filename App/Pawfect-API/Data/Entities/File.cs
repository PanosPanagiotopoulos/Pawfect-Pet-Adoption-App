using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Models.File;

namespace Pawfect_API.Data.Entities
{
	public class File
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public String Id { get; set; }
		public String Filename { get; set; }
		public Double Size { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public String OwnerId { get; set; }
		public String MimeType { get; set; }
		public String FileType { get; set; }
		public String SourceUrl { get; set; }
        public String AwsKey { get; set; }
        public FileSaveStatus FileSaveStatus { get; set; }

		// Commons
		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
	}

	public class FileSaveResult
	{
		public String FileName { get; set; }
		public Models.File.FilePersist File { get; set; }
		public Boolean Success { get; set; }
		public String ErrorMessage { get; set; }
	}

	public class FileInfo
	{
		public String FileId { get; set; }
		public String AwsKey { get; set; }
		public IFormFile TempFile { get; set; }
		public Boolean IsValid { get; set; }
		public String ErrorMessage { get; set; }
	}

	public class UploadResult
	{
		public FilePersist Persist { get; set; }
		public String FileName { get; set; }
		public String Error { get; set; }
	}
}
