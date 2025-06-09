using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.AdoptionApplication
{
    public class AdoptionApplicationValidator : AbstractValidator<AdoptionApplicationPersist>
    {
        public AdoptionApplicationValidator()
        {
            RuleFor(application => application.AnimalId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The animal ID is not in the correct format.");

            RuleFor(application => application.ShelterId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The shelter ID is not in the correct format.");

            RuleFor(application => application.Status)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The application status is not valid. [Available: 1, Pending: 2, Rejected: 3]");

            When(adp => adp.AttachedFilesIds != null, () =>
            {
                RuleForEach(application => application.AttachedFilesIds)
                    .Cascade(CascadeMode.Stop)
                    .Must(RuleFluentValidation.IsObjectId)
                    .WithMessage("The file ID is not in the correct format.");
            });

            RuleFor(application => application.ApplicationDetails)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(15)
                .WithMessage("The adoption application description must have at least 15 characters.");
        }
    }
}