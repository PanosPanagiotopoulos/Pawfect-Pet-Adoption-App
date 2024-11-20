using FluentValidation;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class LocationValidator : AbstractValidator<Location>
    {
        public LocationValidator()
        {
            RuleFor(location => location.Address)
                .Cascade(CascadeMode.Stop)
                // Η διέυθυνση είναι απαραίτητη.
                .NotEmpty()
                .WithMessage("Η διεύθυνση απαιτείται.")
                // Λάθος διέυθυνση.
                .Length(3, 100)
                .WithMessage("Μη έγκυρη διεύθυνση.");

            RuleFor(location => location.Number)
                .Cascade(CascadeMode.Stop)
                // Ο αριθμός διέυθυνσης είναι απαραίτητος.
                .NotEmpty()
                .WithMessage("Ο αριθμός διεύθυνσης απαιτείται.")
                // Λάθος αριθμός διέυθυνσης.
                .Matches(@"^\d+$")
                .WithMessage("Μη έγκυρος αριθμός διεύθυνσης.")
                // Λάθος αριθμός διέυθυνσης.
                .Length(1, 5)
                .WithMessage("Μη έγκυρος αριθμός διεύθυνσης.");

            RuleFor(location => location.City)
                .Cascade(CascadeMode.Stop)
                // Η πόλη είναι απαραίτητη.
                .NotEmpty()
                .WithMessage("Η πόλη απαιτείται.")
                // Λάθος όνομα πόλης.
                .Length(2, 50)
                .WithMessage("Μη έγκυρο όνομα πόλης.");

            RuleFor(location => location.ZipCode)
                .Cascade(CascadeMode.Stop)
                // Ο ταχυδρομικός κώδικας είναι απαραίτητος.
                .NotEmpty()
                .WithMessage("Ο ταχυδρομικός κώδικας απαιτείται.")
                // Λάθος ταχυδρομικός κώδικας.
                .Matches(@"^\d{5}(-\d{4})?$")
                .WithMessage("Μη έγκυρος ταχυδρομικός κώδικας.");
        }
    }
}
