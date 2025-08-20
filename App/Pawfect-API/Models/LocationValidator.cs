using FluentValidation;

namespace Pawfect_API.Models
{
    public class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator()
        {
            RuleFor(location => location.Address)
                .Cascade(CascadeMode.Stop)
                // Η διέυθυνση είναι απαραίτητη.
                .NotEmpty()
                .WithMessage("Address is required")
                // Λάθος διέυθυνση.
                .Length(3, 100)
                .WithMessage("Not a valid address");

            RuleFor(location => location.Number)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Street number is required.")
                .Matches(@"^\d+[A-Za-z]?$")
                .WithMessage("Not a valid street number");

            RuleFor(location => location.City)
                .Cascade(CascadeMode.Stop)
                // Η πόλη είναι απαραίτητη.
                .NotEmpty()
                .WithMessage("City is required")
                // Λάθος όνομα πόλης.
                .Length(2, 50)
                .WithMessage("Not valid city name");

            RuleFor(location => location.ZipCode)
                 .Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithMessage("Zip code is required.")
                 .Matches(@"^\d{5}$")
                 .WithMessage("Invalid zip code");
        }
    }
}
