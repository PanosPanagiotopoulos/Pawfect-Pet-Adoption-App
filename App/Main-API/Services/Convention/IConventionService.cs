﻿using Main_API.Data.Entities.Types.Files;

namespace Main_API.Services.Convention
{
	public interface IConventionService
	{
		Boolean IsValidId(String id);
		String ToExtention(String fileType);
		String ToFileType(String extension);
		String ToMimeType(String extension);
        (Boolean IsValid, String ErrorMessage) IsValidFile(IFormFile file, FilesConfig config);
	}
}
