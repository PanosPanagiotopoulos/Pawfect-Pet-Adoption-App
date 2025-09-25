using FluentValidation;
using Pawfect_API.DevTools;

namespace Pawfect_API.Models.AdoptionApplication
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

            When(adp => adp.Status == Data.Entities.EnumTypes.ApplicationStatus.Rejected, () =>
            {
                RuleFor(application => application.RejectReasson)
                    .Cascade(CascadeMode.Stop)
                    .NotNull()
                    .WithMessage("The Reject reasson is required")
                    .MinimumLength(10)
                    .WithMessage("The Reject reasson must have at least 10 characters.");
            });

            RuleFor(application => application.ApplicationDetails)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(15)
                .WithMessage("The adoption application description must have at least 15 characters.");
        }
    }
}