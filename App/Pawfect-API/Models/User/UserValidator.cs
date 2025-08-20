using FluentValidation;
using Pawfect_API.Data.Entities.EnumTypes;
using Pawfect_API.DevTools;

namespace Pawfect_API.Models.User
{
    public class UserValidator : AbstractValidator<UserPersist>
    {
        public UserValidator()
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

            // The user role is required and must be valid
            RuleFor(user => user.Role)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The user role must be one of: [User: 1, Shelter: 2, Admin: 3].");

            // The phone number is required and must be valid
            RuleFor(user => user.Phone)
                 .Cascade(CascadeMode.Stop)
                 .Matches(@"^\+?[1-9]\d{1,14}$")
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

            // The user's authentication provider is required and must be valid
            RuleFor(user => user.AuthProvider)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The provider must be one of: [Local: 1, Google: 2].");

            // If the authentication provider is not Local, AuthProviderId is required and password must not be provided
            When(user => user.AuthProvider != AuthProvider.Local, () =>
            {
                RuleFor(user => user.AuthProviderId)
                    .Cascade(CascadeMode.Stop)
                    .Must(authProviderId => !String.IsNullOrEmpty(authProviderId))
                    .WithMessage("The user ID from the external service used for registration/login is required.");

                RuleFor(user => user.Password)
                    .Cascade(CascadeMode.Stop)
                    .Must(password => String.IsNullOrEmpty(password))
                    .WithMessage("Do not send a password if authenticated via an external service.");
            });

            When(user => user.AuthProvider == AuthProvider.Local, () =>
            {
                RuleFor(user => user.Password)
                    .Cascade(CascadeMode.Stop)
                    .MinimumLength(7)
                    .WithMessage("The user's password must have at least 7 characters.")
                    .Matches(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{7,}$")
                    .WithMessage("The user's password must have at least 7 characters, including at least one uppercase letter, one number, and one special character.");

                RuleFor(user => user.AuthProviderId)
                    .Cascade(CascadeMode.Stop)
                    .Must(authProviderId => String.IsNullOrEmpty(authProviderId))
                    .WithMessage("Do not send an external service ID if authenticated via an internal service.");
            });
        }
    }
}