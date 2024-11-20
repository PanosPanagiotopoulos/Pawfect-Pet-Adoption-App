using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.Notification
{
    public class NotificationValidator : AbstractValidator<NotificationPersist>
    {
        public NotificationValidator()
        {
            // Το ID του χρήστη είναι απαραίτητο
            RuleFor(notification => notification.UserId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το id δεν είναι σε σωστή μορφή");

            // Ο τύπος της ειδοποίησης είναι απαραίτητος και πρέπει να είναι έγκυρος
            RuleFor(notification => notification.Type)
                .Cascade(CascadeMode.Stop)
                .IsInEnum()
                .WithMessage("Ο τύπος της ειδοποίησης πρέπει να είναι έγκυρος. [ Incoming Message: 1, AdoptionApplication: 2, Report: 3 ]");

            // Το περιεχόμενο της ειδοποίησης είναι απαραίτητο
            RuleFor(notification => notification.Content)
                .Cascade(CascadeMode.Stop)
                .Length(10, 200)
                .WithMessage("Το περιεχόμενο της ειδοποίησης πρέπει να είναι μεταξύ 10-200 χαρακτήρες.");
        }
    }
}
