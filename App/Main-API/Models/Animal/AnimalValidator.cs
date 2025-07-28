using FluentValidation;
using Main_API.DevTools;

namespace Main_API.Models.Animal
{
    public class AnimalValidator : AbstractValidator<AnimalPersist>
    {
        public AnimalValidator()
        {
            RuleFor(animal => animal.Name)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(2)
                .WithMessage("The animal's name must have at least 2 characters.");

            RuleFor(animal => animal.Age)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(0.1, 40)
                .WithMessage("Invalid age in years.");

            RuleFor(animal => animal.Gender)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Invalid animal gender.");

            RuleFor(animal => animal.Description)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(10)
                .WithMessage("The animal's description must have at least 10 characters.");

            RuleFor(animal => animal.Weight)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(0.1, 500)
                .WithMessage("Invalid weight in kilograms.");

            RuleFor(animal => animal.HealthStatus)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(8)
                .WithMessage("Please provide a more detailed description of the animal's health.");

            RuleFor(animal => animal.BreedId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The breed ID is not in the correct format.");

            RuleFor(animal => animal.AnimalTypeId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The animal type ID is not in the correct format.");

            When(animal => animal.AttachedPhotosIds != null, () =>
            {
                RuleForEach(animal => animal.AttachedPhotosIds)
                    .Cascade(CascadeMode.Stop)
                    .Must(RuleFluentValidation.IsObjectId)
                    .WithMessage("Each photo must be a valid file ID.");
            });

            RuleFor(animal => animal.AdoptionStatus)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("The adoption status must be a valid value [Available: 1, Pending: 2, Adopted: 3].");
        }
    }
}