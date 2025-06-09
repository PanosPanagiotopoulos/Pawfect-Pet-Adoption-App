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
                .WithMessage("Breed name must be between 1-100 characters.");

            // Το ID του τύπου ζώου είναι απαραίτητο
            RuleFor(breed => breed.AnimalTypeId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Animal type id must be valid.");

            // Η περιγραφή της ράτσας είναι προαιρετική
            RuleFor(breed => breed.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500)
                .WithMessage("Description must have max 500 characters");
        }
    }
}
