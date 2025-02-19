using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;

namespace Pawfect_Pet_Adoption_App_API.Models.Message
{
    public class MessageValidator : AbstractValidator<MessagePersist>
    {
        public MessageValidator()
        {
            // Το ID της συνομιλίας είναι απαραίτητο
            RuleFor(message => message.ConversationId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID της συνομιλίας δεν είναι σε σωστή μορφή.");

            // Το ID του αποστολέα είναι απαραίτητο
            RuleFor(message => message.SenderId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του αποστολέα δεν είναι σε σωστή μορφή.");

            // Το ID του παραλήπτη είναι απαραίτητο
            RuleFor(message => message.RecipientId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το ID του παραλήπτη δεν είναι σε σωστή μορφή.");

            // Το περιεχόμενο του μηνύματος είναι απαραίτητο
            RuleFor(message => message.Content)
                .Cascade(CascadeMode.Stop)
                .Length(1, 1000)
                .WithMessage("Το περιεχόμενο του μηνύματος πρέπει να είναι μεταξύ 1-1000 χαρακτήρες.");
        }
    }
}
