using FluentValidation;
namespace Pawfect_Pet_Adoption_App_API.Models.AnimalType
{
    public class AnimalTypeValidator : AbstractValidator<AnimalTypePersist>
    {
        public AnimalTypeValidator()
        {
            // Το όνομα του τύπου ζώου είναι απαραίτητο
            RuleFor(animalType => animalType.Name)
                .Cascade(CascadeMode.Stop)
                .Length(1, 100)
                .WithMessage("Το όνομα του τύπου ζώου πρέπει να είναι μεταξύ 1-100 χαρακτήρες.");

            // Η περιγραφή του τύπου ζώου είναι προαιρετική
            RuleFor(animalType => animalType.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500)
                .WithMessage("Η περιγραφή του τύπου ζώου δεν πρέπει να υπερβαίνει τους 500 χαρακτήρες.");
        }
    }
}
