using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.Shelter
{
    public class ShelterValidator : AbstractValidator<ShelterPersist>
    {
        public ShelterValidator()
        {
            // Ελέγξτε αν το όνομα του καταφυγίου δεν είναι κενό
            RuleFor(shelter => shelter.ShelterName)
            .MinimumLength(2)
            .WithMessage("Το όνομα του καταφυγίου πρέπει να έχει τουλάχιστον 2 χαρακτήρες");

            // Ελέγξτε αν η περιγραφή δεν είναι κενή
            RuleFor(shelter => shelter.Description)
                .MinimumLength(10)
                .WithMessage("Η περιγραφή πρέπει να έχει τουλάχιστον 10 χαρακτήρες");

            When(shelter => !String.IsNullOrEmpty(shelter.Website), () =>
            {
                // Ελέγξτε αν το link της ιστοσελίδας είναι σωστά διαμορφωμένο
                RuleFor(shelter => shelter.Website)
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                .WithMessage("Μη έγκυρο link ιστοσελίδας");
            });

            When(x => x.VerificationStatus != default(VerificationStatus), () =>
            {
                RuleFor(x => x.VerificationStatus)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Η κατάσταση έγκρισης λογαριασμού πρέπει να είναι έγκυρη τιμή. [ Pending: 1, Resolved: 2, Rejected: 3 ]");
            });

            RuleFor(shelter => shelter.SocialMedia)
                .SetValidator(new SocialMediaValidator());

            RuleFor(shelter => shelter.OperatingHours)
                .SetValidator(new OperatingHoursValidator());
        }
    }
}
