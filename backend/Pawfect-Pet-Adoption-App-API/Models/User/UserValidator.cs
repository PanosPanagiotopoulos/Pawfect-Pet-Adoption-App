using FluentValidation;
using Pawfect_Pet_Adoption_App_API.Data.Entities.EnumTypes;

namespace Pawfect_Pet_Adoption_App_API.Models.User
{
    public class UserValidator : AbstractValidator<UserPersist>
    {
        public UserValidator()
        {
            // Το email είναι απαραίτητο και πρέπει να είναι έγκυρο
            RuleFor(user => user.Email)
                .Cascade(CascadeMode.Stop)
                .EmailAddress()
                .WithMessage("Παρακαλώ εισάγετε έναν έγκυρο email.");

            // Το ονοματεπώνυμο είναι απαραίτητο και πρέπει να έχει τουλάχιστον 5 χαρακτήρες
            RuleFor(user => user.FullName)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(5)
                .WithMessage("Το ονοματεπώνυμο δεν μπορεί να έχει λιγότερο απο 5 χαρακτήρες.");

            // Ο ρόλος του χρήστη είναι απαραίτητος και πρέπει να είναι έγκυρος
            RuleFor(user => user.Role)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Ο ρόλος του χρήστη πρέπει να είναι μεταξύ: (User : 1, Shelter : 2, Admin : 3).");

            // Ο αριθμός τηλεφώνου είναι απαραίτητος και πρέπει να είναι έγκυρος
            RuleFor(user => user.Phone)
                .Cascade(CascadeMode.Stop)
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Παρακαλώ εισάγετε έναν έγκυρο αριθμό τηλεφώνου.");

            // Εάν υπάρχει τοποθεσία, πρέπει να είναι έγκυρη σύμφωνα με τους κανόνες δημιουργίας
            RuleFor(user => user.Location)
            .SetValidator(new LocationValidator());

            // Ο τρόπος πρόσβασης του χρήστη είναι απαραίτητος και πρέπει να είναι έγκυρος
            RuleFor(user => user.AuthProvider)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Ο Provider πρέπει να είναι [ Local : 1, Google: 2 ]");

            // Εάν ο τρόπος πρόσβασης δεν είναι Local, το AuthProviderId είναι απαραίτητο και δεν πρέπει να υπάρχει κωδικός
            When(user => user.AuthProvider != AuthProvider.Local, () =>
            {
                RuleFor(user => user.AuthProviderId)
                .Cascade(CascadeMode.Stop)
                .Must(authProviderId => !string.IsNullOrEmpty(authProviderId))
                .WithMessage("To id του χρήστη στην εξωτερική υπηρεσία που επέλεξε να εγγραφεί/συνδεθεί είναι απαραίτητο.");

                RuleFor(user => user.Password)
                    .Cascade(CascadeMode.Stop)
                    .Must(password => string.IsNullOrEmpty(password))
                    .WithMessage("Μην στέλνεται κωδικό εφόσον έχετε ταυτοποιηθεί απο εξωτερική υπηρεσία.");
            });

            // Εάν ο τρόπος πρόσβασης είναι Local, ο κωδικός είναι απαραίτητος και πρέπει να πληροί συγκεκριμένες προϋποθέσεις
            When(user => user.AuthProvider == AuthProvider.Local, () =>
            {
                RuleFor(user => user.Password)
               .Cascade(CascadeMode.Stop)
               .MinimumLength(7)
               .WithMessage("Ο κωδικός ενώς χρήστη πρέπει να έχει τουλάχιστον 6 χαρακτήρες.")
               .Matches(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]).{7,}$")
               .WithMessage("Ο κωδικός ενώς χρήστη πρέπει να έχει τουλάχιστον 6 χαρακτήρες, τουλάχιστον 1 κεφαλαίο, έναν αριθμό και έναν ειδικό χαρακτήρα.");

                RuleFor(user => user.AuthProviderId)
                    .Cascade(CascadeMode.Stop)
                    .Must(authProviderId => string.IsNullOrEmpty(authProviderId))
                    .WithMessage("Μην στέλνεται κωδικό id εξωτερικής υπηρεσίας εφόσον έχετε ταυτοποιηθεί απο εσωτερική υπηρεσία.");
            });
        }
    }
}
