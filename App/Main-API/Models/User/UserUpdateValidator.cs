using FluentValidation;
using Main_API.Models;
using Main_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.User
{
    public class UserUpdateValidator : AbstractValidator<UserUpdate>
    {
        public UserUpdateValidator()
        {
            // The email is required and must be valid
            RuleFor(user => user.Email)
                .Cascade(CascadeMode.Stop)
                .EmailAddress()
                .WithMessage("Please enter a valid email address.");

            // The full name is required and must have at least 5 characters
            RuleFor(user => user.FullName)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(5)
                .WithMessage("The full name cannot have fewer than 5 characters.");

          
            // The phone number is required and must be valid
            RuleFor(user => user.Phone)
                .Cascade(CascadeMode.Stop)
                .Matches(@"^\+?[1-9]\Double{1,14}$")
                .WithMessage("Please enter a valid phone number.");

            // If a location is provided, it must be valid according to creation rules
            RuleFor(user => user.Location)
                .SetValidator(new LocationValidator());

            When(user => !String.IsNullOrEmpty(user.ProfilePhotoId), () =>
            {
                RuleFor(user => user.ProfilePhotoId)
                    .Cascade(CascadeMode.Stop)
                    .Must(RuleFluentValidation.IsObjectId)
                    .WithMessage("The profile photo is not valid.");
            });
        }
    }
}
