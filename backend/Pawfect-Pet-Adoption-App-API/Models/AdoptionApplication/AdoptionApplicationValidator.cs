using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;
namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
    public class AdoptionApplicationValidator : AbstractValidator<AdoptionApplicationPersist>
    {
        public AdoptionApplicationValidator()
        {
            RuleFor(application => application.UserId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του χρήστη δεν είναι σε σωστή μορφή.");

            RuleFor(application => application.AnimalId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του ζώου δεν είναι σε σωστή μορφή.");

            RuleFor(application => application.ShelterId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του καταφυγίου δεν είναι σε σωστή μορφή.");

            RuleFor(application => application.Status)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Η κατάσταση της αίτησης δεν είναι έγκυρη. [ Available: 1, Pending: 2, Rejected: 3 ]");

            When(adp => adp.AttachedFilesIds != null, () =>
            {
                RuleForEach(application => application.AttachedFilesIds)
				.Cascade(CascadeMode.Stop)
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Το ID του file δεν είναι σε σωστή μορφή.");
			});
            

			RuleFor(application => application.ApplicationDetails)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(15)
                .WithMessage("Η περιγραφή της αίτησης υιοθεσίας πρέπει να έχει τουλάχιστον 15 χαρακτήρες.");
        }
    }
}
