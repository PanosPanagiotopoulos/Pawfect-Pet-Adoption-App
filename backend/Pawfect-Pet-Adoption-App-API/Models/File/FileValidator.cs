using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.Types.Files;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.File
{
	public class FileValidator : AbstractValidator<FilePersist>
	{
		public FileValidator()
		{
			// Validate Id (must be a valid ObjectId)
			RuleFor(file => file.Id)
				.Cascade(CascadeMode.Stop)
				.Must(id => RuleFluentValidation.IsObjectId(id) || String.IsNullOrEmpty(id))
				.WithMessage("Το ID του αρχείου δεν είναι σε σωστή μορφή.");

			// Validate Filename (length between 1 and 100 characters)
			RuleFor(file => file.Filename)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Το όνομα του αρχείου είναι απαραίτητο.")
				.Length(1, 100)
				.WithMessage("Το όνομα του αρχείου πρέπει να είναι μεταξύ 1-100 χαρακτήρες.");

			// Validate Size (must be greater than 0)
			RuleFor(file => file.Size)
				.Cascade(CascadeMode.Stop)
				.GreaterThan(0)
				.WithMessage("Το μέγεθος του αρχείου πρέπει να είναι μεγαλύτερο από 0.");

			// Validate OwnerId (must be a valid ObjectId)
			RuleFor(file => file.OwnerId)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Το ID του ιδιοκτήτη είναι απαραίτητο.")
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Το ID του ιδιοκτήτη δεν είναι σε σωστή μορφή.");

			// Validate MimeType (length between 1 and 50 characters)
			RuleFor(file => file.MimeType)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Το MimeType του αρχείου είναι απαραίτητο.")
				.Length(1, 50)
				.WithMessage("Το MimeType του αρχείου πρέπει να είναι μεταξύ 1-50 χαρακτήρες.");

			RuleFor(file => file.FileType)
				.Cascade(CascadeMode.Stop)
				.Must(fType => !String.IsNullOrEmpty(fType))
				.WithMessage("Ο τύπος του αρχείου δεν είναι έγκυρος.");

			When(file => file.FileSaveStatus.HasValue, () =>
			{
				// Validate FileType (must be a valid enum value)
				RuleFor(file => file.FileSaveStatus)
					.Cascade(CascadeMode.Stop)
					.IsInEnum()
					.WithMessage("Ο τύπος αποθηκευσης αρχειου δεν είναι έγκυρος.");
			});

			// Validate SourceUrl (must be a valid URL)
			RuleFor(file => file.SourceUrl)
				.Cascade(CascadeMode.Stop)
				.NotEmpty()
				.WithMessage("Το URL της πηγής είναι απαραίτητο.")
				.Must(RuleFluentValidation.IsUrl)
				.WithMessage("Το URL της πηγής δεν είναι έγκυρο.");
		}
	}
}