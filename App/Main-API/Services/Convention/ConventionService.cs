using MongoDB.Bson;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.DevTools;
using System.Reflection;

namespace Pawfect_Pet_Adoption_App_API.Services.Convention
{
	public class ConventionService : IConventionService
	{
		/// <summary>
		/// Checks if a String is a valid MongoDB ObjectId.
		/// </summary>
		/// <param name="id">The String to validate.</param>
		/// <returns>True if the String is a valid ObjectId, otherwise false.</returns>
		public Boolean IsValidId(String id) => !String.IsNullOrEmpty(id) && RuleFluentValidation.IsObjectId(id);

		/// <summary>
		/// Maps a FileType enum value to its corresponding file extension.
		/// </summary>
		/// <param name="fileType">The FileType enum value.</param>
		/// <returns>The corresponding file extension as a String.</returns>
		public String ToExtention(String fileType)
		{
			Dictionary<String, String> fileTypeMappings = new Dictionary<String, String>
			{
				{ FileType.PDF, ".pdf" },
				{ FileType.JPG, ".jpg" },
				{ FileType.JPEG, ".jpeg" },
				{ FileType.PNG, ".png" },
				{ FileType.GIF, ".gif" },
				{ FileType.MP4, ".mp4" },
				{ FileType.MOV, ".mov" },
				{ FileType.AVI, ".avi" },
				{ FileType.MP3, ".mp3" },
				{ FileType.WAV, ".wav" },
				{ FileType.FLAC, ".flac" },
				{ FileType.OGG, ".ogg" },
				{ FileType.WEBM, ".webm" },
				{ FileType.WMV, ".wmv" },
				{ FileType.EXCEL, ".xlsx" }
			};

			return fileTypeMappings.TryGetValue(fileType, out String extension)
				? extension
				: throw new ArgumentException($"Unsupported FileType: {fileType}");
		}

		public String ToFileType(String extension)
		{
			Dictionary<String, String> extensionToFileTypeMappings = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
			{
				{ ".pdf", FileType.PDF },
				{ ".jpg", FileType.JPG },
				{ ".jpeg", FileType.JPEG },
				{ ".png", FileType.PNG },
				{ ".gif", FileType.GIF },
				{ ".mp4", FileType.MP4 },
				{ ".mov", FileType.MOV },
				{ ".avi", FileType.AVI },
				{ ".mp3", FileType.MP3 },
				{ ".wav", FileType.WAV },
				{ ".flac", FileType.FLAC },
				{ ".ogg", FileType.OGG },
				{ ".webm", FileType.WEBM },
				{ ".wmv", FileType.WMV },
				{ ".xlsx", FileType.EXCEL }
			};

			return extensionToFileTypeMappings.TryGetValue(extension, out String fileType)
				? fileType
				: throw new ArgumentException($"Unsupported file extension: {extension}");
		}

        public String ToMimeType(String extension)
        {
            Dictionary<String, String> extensionToMimeTypeMappings = new Dictionary<String, String>(StringComparer.OrdinalIgnoreCase)
			{
				{ ".pdf", "application/pdf" },
				{ ".jpg", "image/jpeg" },
				{ ".jpeg", "image/jpeg" },
				{ ".png", "image/png" },
				{ ".gif", "image/gif" },
				{ ".mp4", "video/mp4" },
				{ ".mov", "video/quicktime" },
				{ ".avi", "video/x-msvideo" },
				{ ".mp3", "audio/mpeg" },
				{ ".wav", "audio/wav" },
				{ ".flac", "audio/flac" },
				{ ".ogg", "audio/ogg" },
				{ ".webm", "video/webm" },
				{ ".wmv", "video/x-ms-wmv" },
				{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
			};

            return extensionToMimeTypeMappings.TryGetValue(extension, out String mimeType)
                ? mimeType
                : throw new ArgumentException($"Unsupported file extension: {extension}");
        }

        public (Boolean IsValid, String ErrorMessage) IsValidFile(IFormFile file, FilesConfig config)
		{
			if (file == null || file.Length == 0)
				return (false, "File is null or empty.");

			if (file.Length > config.MaxFileSizeBytes)
				return (false, $"File size exceeds maximum limit of {config.MaxFileSizeBytes / (1024 * 1024)}MB.");

			String extension = Path.GetExtension(file.FileName).ToLower();
			FileTypeConfig fileTypeConfig = config.AllowedFileTypes.FirstOrDefault(ft =>
				ft.MimeTypes.Contains(file.ContentType) || ft.Extensions.Contains(extension));

			if (fileTypeConfig == null)
				return (false, "Unsupported file type.");

			// Validate FileType against static FileType class constants
			List<String> validFileTypes = 
				[..
					typeof(FileType).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
					.Where(f => f.IsLiteral && !f.IsInitOnly)
					.Select(f => f.GetValue(null).ToString())
				];

			if (!validFileTypes.Contains(fileTypeConfig.FileType.ToString()))
				return (false, $"File type {fileTypeConfig.FileType} is not a valid FileType value.");


			return (true, null);
		}
	}
}
