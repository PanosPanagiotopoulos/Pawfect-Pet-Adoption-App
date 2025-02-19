using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;

namespace Pawfect_Pet_Adoption_App_API.Models
{
    public class OperatingHoursValidator : AbstractValidator<OperatingHours>
    {
        private readonly String OperatingHoursRegex = @"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$";
        private readonly String OperatingHoursErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ";

        public OperatingHoursValidator()
        {
            When(operatingHours => operatingHours != null, () =>
            {
                // Validate Monday operating hours
                RuleFor(hours => hours.Monday)
                    .NotEmpty().WithMessage("Η Δευτέρα απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Tuesday operating hours
                RuleFor(hours => hours.Tuesday)
                    .NotEmpty().WithMessage("Η Τρίτη απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Wednesday operating hours
                RuleFor(hours => hours.Wednesday)
                    .NotEmpty().WithMessage("Η Τετάρτη απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Thursday operating hours
                RuleFor(hours => hours.Thursday)
                    .NotEmpty().WithMessage("Η Πέμπτη απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Friday operating hours
                RuleFor(hours => hours.Friday)
                    .NotEmpty().WithMessage("Η Παρασκευή απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Saturday operating hours
                RuleFor(hours => hours.Saturday)
                    .NotEmpty().WithMessage("Το Σάββατο απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);

                // Validate Sunday operating hours
                RuleFor(hours => hours.Sunday)
                    .NotEmpty().WithMessage("Η Κυριακή απαιτείται.")
                    .Matches(OperatingHoursRegex)
                    .WithMessage(OperatingHoursErrorMessage);
            });
        }
    }
}
