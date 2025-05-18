using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;
namespace Pawfect_Pet_Adoption_App_API.Models.Breed
{
    public class BreedValidator : AbstractValidator<BreedPersist>
    {
        public BreedValidator()
        {
            // Το όνομα της ράτσας είναι απαραίτητο
            RuleFor(breed => breed.Name)
                .Cascade(CascadeMode.Stop)
                .Length(1, 100)
                .WithMessage("Το όνομα της ράτσας πρέπει να είναι μεταξύ 1-100 χαρακτήρες.");

            // Το ID του τύπου ζώου είναι απαραίτητο
            RuleFor(breed => breed.AnimalTypeId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του τύπου ζώου δεν είναι σε σωστή μορφή.");

            // Η περιγραφή της ράτσας είναι προαιρετική
            RuleFor(breed => breed.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500)
                .WithMessage("Η περιγραφή της ράτσας δεν πρέπει να υπερβαίνει τους 500 χαρακτήρες.");
        }
    }
}
