using FluentValidation;
using Main_API.Data.Entities.EnumTypes;

namespace Main_API.Models.Shelter
{
    public class ShelterValidator : AbstractValidator<ShelterPersist>
    {
        public ShelterValidator()
        {
            // Check if the shelter name is not empty
            RuleFor(shelter => shelter.ShelterName)
                .MinimumLength(2)
                .WithMessage("The shelter name must have at least 2 characters.");

            // Check if the description is not empty
            RuleFor(shelter => shelter.Description)
                .MinimumLength(10)
                .WithMessage("The description must have at least 10 characters.");

            When(shelter => !String.IsNullOrEmpty(shelter.Website), () =>
            {
                // Check if the website link is properly formatted
                RuleFor(shelter => shelter.Website)
                    .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                    .WithMessage("Invalid website link.");
            });

            When(x => x.VerificationStatus != default(VerificationStatus), () =>
            {
                RuleFor(x => x.VerificationStatus)
                    .Cascade(CascadeMode.Stop)
                    .IsInEnum()
                    .WithMessage("The account verification status must be a valid value. [Pending: 1, Resolved: 2, Rejected: 3]");
            });

            RuleFor(shelter => shelter.SocialMedia)
                .SetValidator(new SocialMediaValidator());

            RuleFor(shelter => shelter.OperatingHours)
                .SetValidator(new OperatingHoursValidator());
        }
    }
}