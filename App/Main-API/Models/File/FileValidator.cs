using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.File
{
	public class FileValidator : AbstractValidator<FilePersist>
	{
		public FileValidator()
		{
			// Validate Filename (length between 1 and 100 characters)
			RuleFor(file => file.Filename)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Filename is needed")
				.Length(1, 100)
				.WithMessage("File must have length from 1-100 characters");

			// Validate Size (must be greater than 0)
			RuleFor(file => file.Size)
				.Cascade(CascadeMode.Stop)
				.GreaterThan(0)
				.WithMessage("File size must be more than 0");

			// Validate OwnerId (must be a valid ObjectId)
			RuleFor(file => file.OwnerId)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Owner id is required")
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Owner Id must be valid");

			// Validate MimeType (length between 1 and 50 characters)
			RuleFor(file => file.MimeType)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("MimeType is required")
				.Length(1, 50)
				.WithMessage("Mime type must be between 1, 50 characters");

			RuleFor(file => file.FileType)
				.Cascade(CascadeMode.Stop)
				.Must(fType => !String.IsNullOrEmpty(fType))
				.WithMessage("File type must be valid");

			When(file => file.FileSaveStatus.HasValue, () =>
			{
				// Validate FileType (must be a valid enum value)
				RuleFor(file => file.FileSaveStatus)
					.Cascade(CascadeMode.Stop)
					.IsInEnum()
					.WithMessage("File save status must be valid");
			});

			// Validate SourceUrl (must be a valid URL)
			RuleFor(file => file.SourceUrl)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Source Url is need")
				.Must(RuleFluentValidation.IsUrl)
				.WithMessage("Source url must be valid");
		}
	}
}