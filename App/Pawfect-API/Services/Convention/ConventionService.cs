using Microsoft.Extensions.Options;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.Data.Entities.Types.Files;
using Pawfect_API.DevTools;

namespace Pawfect_API.Services.Convention
{
    public class ConventionService : IConventionService
    {
        private readonly FilesConfig _filesConfig;

        public ConventionService(IOptions<FilesConfig> filesOptions)
        {
            _filesConfig = filesOptions.Value;
        }

        /// <summary>
        /// Checks if a String is a valid MongoDB ObjectId.
        /// </summary>
        /// <param name="id">The String to validate.</param>
        /// <returns>True if the String is a valid ObjectId, otherwise false.</returns>
        public Boolean IsValidId(String id) => !String.IsNullOrEmpty(id) && RuleFluentValidation.IsObjectId(id);

        /// <summary>
        /// Maps a FileType to its corresponding file extension using config.
        /// </summary>
        /// <param name="fileType">The FileType value.</param>
        /// <returns>The corresponding file extension as a String.</returns>
        public String ToExtention(String fileType)
        {
            FileTypeConfig config = _filesConfig.AllowedFileTypes
                .FirstOrDefault(ft => ft.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase));

            if (config == null)
                throw new ArgumentException($"Unsupported FileType: {fileType}");

            // Return the first extension from config
            return config.Extensions.FirstOrDefault()
                ?? throw new ArgumentException($"No extension found for FileType: {fileType}");
        }

        /// <summary>
        /// Maps a file extension to its corresponding FileType using config.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The corresponding FileType as a String.</returns>
        public String ToFileType(String extension)
        {
            if (String.IsNullOrEmpty(extension))
                throw new ArgumentException("Extension cannot be null or empty");

            FileTypeConfig config = _filesConfig.AllowedFileTypes
                .FirstOrDefault(ft => ft.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            if (config == null)
                throw new ArgumentException($"Unsupported file extension: {extension}");

            return config.FileType;
        }

        /// <summary>
        /// Maps a file extension to its corresponding MIME type using config.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The corresponding MIME type as a String.</returns>
        public String ToMimeType(String extension)
        {
            if (String.IsNullOrEmpty(extension))
                throw new ArgumentException("Extension cannot be null or empty");

            FileTypeConfig config = _filesConfig.AllowedFileTypes
                .FirstOrDefault(ft => ft.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            if (config == null)
                throw new ArgumentException($"Unsupported file extension: {extension}");

            // Return the first MIME type from config
            return config.MimeTypes.FirstOrDefault()
                ?? throw new ArgumentException($"No MIME type found for extension: {extension}");
        }

        /// <summary>
        /// Gets all extensions for a given FileType using config.
        /// </summary>
        /// <param name="fileType">The FileType value.</param>
        /// <returns>List of extensions for the FileType.</returns>
        public List<String> GetExtensionsForFileType(String fileType)
        {
            FileTypeConfig config = _filesConfig.AllowedFileTypes
                .FirstOrDefault(ft => ft.FileType.Equals(fileType, StringComparison.OrdinalIgnoreCase));

            return config?.Extensions ?? new List<String>();
        }

        /// <summary>
        /// Gets all MIME types for a given extension using config.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>List of MIME types for the extension.</returns>
        public List<String> GetMimeTypesForExtension(String extension)
        {
            if (String.IsNullOrEmpty(extension))
                return new List<String>();

            FileTypeConfig config = _filesConfig.AllowedFileTypes
                .FirstOrDefault(ft => ft.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));

            return config?.MimeTypes ?? new List<String>();
        }

        /// <summary>
        /// Validates if a file is allowed based on the files configuration.
        /// </summary>
        /// <param name="file">The file to validate.</param>
        /// <param name="config">The files configuration.</param>
        /// <returns>Validation result with success status and error message.</returns>
        public (Boolean IsValid, String ErrorMessage) IsValidFile(IFormFile file, FilesConfig config)
        {
            if (file == null || file.Length == 0)
                return (false, "File is null or empty.");

            if (file.Length > config.MaxFileSizeBytes)
                return (false, $"File size exceeds maximum limit of {config.MaxFileSizeBytes / (1024 * 1024)}MB.");

            String extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (String.IsNullOrEmpty(extension))
                return (false, "File has no extension.");

            // Find matching file type configuration
            FileTypeConfig fileTypeConfig = config.AllowedFileTypes.FirstOrDefault(ft =>
                ft.Extensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)) ||
                ft.MimeTypes.Any(mime => mime.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)));

            if (fileTypeConfig == null)
                return (false, $"Unsupported file type. Extension: {extension}, MIME type: {file.ContentType}");

            return (true, null);
        }

        /// <summary>
        /// Determines if a file type should have private access based on configuration.
        /// </summary>
        /// <param name="fileType">The FileType value.</param>
        /// <returns>FileAccessType (Public or Private).</returns>
        public FileAccessType ExtractAccessType(String fileType)
        {
            if (String.IsNullOrEmpty(fileType))
                return FileAccessType.Public;

            Boolean isPrivate = _filesConfig.PrivateFileTypes?.Contains(fileType, StringComparer.OrdinalIgnoreCase) ?? false;
            return isPrivate ? FileAccessType.Private : FileAccessType.Public;
        }

        /// <summary>
        /// Gets all allowed file types from configuration.
        /// </summary>
        /// <returns>List of all allowed FileType values.</returns>
        public List<String> GetAllowedFileTypes()
        {
            return _filesConfig.AllowedFileTypes?.Select(ft => ft.FileType).ToList() ?? new List<String>();
        }

        /// <summary>
        /// Gets all allowed extensions from configuration.
        /// </summary>
        /// <returns>List of all allowed file extensions.</returns>
        public List<String> GetAllowedExtensions()
        {
            return _filesConfig.AllowedFileTypes?.SelectMany(ft => ft.Extensions).Distinct().ToList() ?? new List<String>();
        }

        /// <summary>
        /// Gets all allowed MIME types from configuration.
        /// </summary>
        /// <returns>List of all allowed MIME types.</returns>
        public List<String> GetAllowedMimeTypes()
        {
            return _filesConfig.AllowedFileTypes?.SelectMany(ft => ft.MimeTypes).Distinct().ToList() ?? new List<String>();
        }

        /// <summary>
        /// Checks if an extension is allowed based on configuration.
        /// </summary>
        /// <param name="extension">The file extension to check.</param>
        /// <returns>True if the extension is allowed, false otherwise.</returns>
        public Boolean IsExtensionAllowed(String extension)
        {
            if (String.IsNullOrEmpty(extension))
                return false;

            return _filesConfig.AllowedFileTypes?.Any(ft =>
                ft.Extensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase))) ?? false;
        }

        /// <summary>
        /// Checks if a MIME type is allowed based on configuration.
        /// </summary>
        /// <param name="mimeType">The MIME type to check.</param>
        /// <returns>True if the MIME type is allowed, false otherwise.</returns>
        public Boolean IsMimeTypeAllowed(String mimeType)
        {
            if (String.IsNullOrEmpty(mimeType))
                return false;

            return _filesConfig.AllowedFileTypes?.Any(ft =>
                ft.MimeTypes.Any(mime => mime.Equals(mimeType, StringComparison.OrdinalIgnoreCase))) ?? false;
        }
    }
}