using FluentValidation;

using Pawfect_Pet_Adoption_App_API.Data.Entities.HelperModels;

using System.Text.RegularExpressions;

namespace Pawfect_Pet_Adoption_App_API.Models
{
	public class OperatingHoursValidator : AbstractValidator<OperatingHours>
	{
		private readonly String OperatingHoursRegex = @"^(?:[01]\d|2[0-3]):[0-5]\d,(?:[01]\d|2[0-3]):[0-5]\d$";
		private readonly String OperatingHoursErrorMessage = "Η μορφή ώρας πρέπει να είναι ΩΩ:ΛΛ,ΩΩ:ΛΛ ή 'closed'";

		public OperatingHoursValidator()
		{
			When(operatingHours => operatingHours != null, () =>
			{
				// Validate Monday operating hours
				RuleFor(hours => hours.Monday)
					.NotEmpty().WithMessage("Η Δευτέρα απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Tuesday operating hours
				RuleFor(hours => hours.Tuesday)
					.NotEmpty().WithMessage("Η Τρίτη απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Wednesday operating hours
				RuleFor(hours => hours.Wednesday)
					.NotEmpty().WithMessage("Η Τετάρτη απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Thursday operating hours
				RuleFor(hours => hours.Thursday)
					.NotEmpty().WithMessage("Η Πέμπτη απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Friday operating hours
				RuleFor(hours => hours.Friday)
					.NotEmpty().WithMessage("Η Παρασκευή απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Saturday operating hours
				RuleFor(hours => hours.Saturday)
					.NotEmpty().WithMessage("Το Σάββατο απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);

				// Validate Sunday operating hours
				RuleFor(hours => hours.Sunday)
					.NotEmpty().WithMessage("Η Κυριακή απαιτείται.")
					.Must(BeValidOperatingHours)
					.WithMessage(OperatingHoursErrorMessage);
			});
		}

		private Boolean BeValidOperatingHours(String hours)
		{
			return Regex.IsMatch(hours, OperatingHoursRegex) || String.Equals(hours, "closed", StringComparison.OrdinalIgnoreCase);
		}
	}
}