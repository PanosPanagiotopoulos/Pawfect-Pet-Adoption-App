using Pawfect_Notifications.DevTools;
using System.Reflection;

namespace Pawfect_Notifications.Services.Convention
{
	public class ConventionService : IConventionService
	{
		/// <summary>
		/// Checks if a String is a valid MongoDB ObjectId.
		/// </summary>
		/// <param name="id">The String to validate.</param>
		/// <returns>True if the String is a valid ObjectId, otherwise false.</returns>
		public Boolean IsValidId(String id) => !String.IsNullOrEmpty(id) && RuleFluentValidation.IsObjectId(id);

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
	}
}
