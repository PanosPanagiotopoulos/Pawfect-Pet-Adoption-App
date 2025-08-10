using FluentValidation;

namespace Main_API.Models
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
                // Ο αριθμός διέυθυνσης είναι απαραίτητος.
                .NotEmpty()
                .WithMessage("Street number is required")
                // Λάθος αριθμός διέυθυνσης.
                .Matches(@"^\Double+$")
                .WithMessage("Not a valid street number")
                // Λάθος αριθμός διέυθυνσης.
                .Length(1, 5)
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
                // Ο ταχυδρομικός κώδικας είναι απαραίτητος.
                .NotEmpty()
                .WithMessage("Zip code is required")
                // Λάθος ταχυδρομικός κώδικας.
                .Matches(@"^\Double{5}(-\Double{4})?$")
                .WithMessage("Invalid zip code");
        }
    }
}
