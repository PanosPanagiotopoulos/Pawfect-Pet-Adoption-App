using FluentValidation;

using Pawfect_Pet_Adoption_App_API.DevTools;
namespace Pawfect_Pet_Adoption_App_API.Models.Animal
{
	public class AnimalValidator : AbstractValidator<AnimalPersist>
	{
		public AnimalValidator()
		{
			RuleFor(animal => animal.Name)
				.Cascade(CascadeMode.Stop)
				.MinimumLength(2)
				.WithMessage("Το όνομα του ζώου πρέπει να έχει τουλάχιστον 2 χαρακτήρες.");

			RuleFor(animal => animal.Age)
				.Cascade(CascadeMode.Stop)
				.InclusiveBetween(0.1, 40)
				.WithMessage("Λάθος αριθμός ηλικείας σε χρόνια.");

			RuleFor(animal => animal.Gender)
				.Cascade(CascadeMode.Stop)
				.IsInEnum()
				.WithMessage("Λάθος φύλο ζώου");

			RuleFor(animal => animal.Description)
				.Cascade(CascadeMode.Stop)
				.MinimumLength(10)
				.WithMessage("Η περιγραφή του ζώου πρέπει να έχει τουλάχιστον 10 χαρακτήρες.");

			RuleFor(animal => animal.Weight)
				.Cascade(CascadeMode.Stop)
				.InclusiveBetween(0.1, 150)
				.WithMessage("Λάθος αριθμός βάρους σε κιλά.");

			RuleFor(animal => animal.HealthStatus)
				.Cascade(CascadeMode.Stop)
				.MinimumLength(8)
				.WithMessage("Παρακαλώ αναγράψτε μια αναλυτικότερη καταφραγή της υγείας του ζωόυ.");

			RuleFor(animal => animal.ShelterId)
				.Cascade(CascadeMode.Stop)
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Το ID του καταφυγίου δεν είναι σε σωστή μορφή.");

			RuleFor(animal => animal.BreedId)
				.Cascade(CascadeMode.Stop)
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Το ID της ράτσας δεν είναι σε σωστή μορφή.");

			RuleFor(animal => animal.AnimalTypeId)
				.Cascade(CascadeMode.Stop)
				.Must(RuleFluentValidation.IsObjectId)
				.WithMessage("Το ID του τύπου ζώου δεν είναι σε σωστή μορφή.");

			When(animal => animal.AttachedPhotosIds != null, () =>
			{
				RuleForEach(animal => animal.AttachedPhotosIds)
					.Cascade(CascadeMode.Stop)
					.Must(RuleFluentValidation.IsObjectId)
					.WithMessage("Κάθε φωτογραφία πρέπει να είναι έγκυρο file id.");
			});
			
			RuleFor(animal => animal.AdoptionStatus)
			.Cascade(CascadeMode.Stop)
			.IsInEnum()
			.WithMessage("Η κατάσταση υιοθεσίας πρέπει να είναι έγκυρη τιμή [Available : 1, Pending : 2, Adopted: 3 ].");
		}
	}
}
