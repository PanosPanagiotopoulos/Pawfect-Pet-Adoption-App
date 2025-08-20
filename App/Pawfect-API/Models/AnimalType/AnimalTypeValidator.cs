using FluentValidation;
namespace Pawfect_API.Models.AnimalType
{
    public class AnimalTypeValidator : AbstractValidator<AnimalTypePersist>
    {
        public AnimalTypeValidator()
        {
            // The name of the animal type is required
            RuleFor(animalType => animalType.Name)
                .Cascade(CascadeMode.Stop)
                .Length(1, 100)
                .WithMessage("The name of the animal type must be between 1 and 100 characters.");

            // The description of the animal type is optional
            RuleFor(animalType => animalType.Description)
                .Cascade(CascadeMode.Stop)
                .MaximumLength(500)
                .WithMessage("The description of the animal type must not exceed 500 characters.");
        }
    }
}