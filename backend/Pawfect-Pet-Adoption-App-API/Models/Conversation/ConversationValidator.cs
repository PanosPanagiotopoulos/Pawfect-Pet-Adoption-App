using FluentValidation;
using Pawfect_Pet_Adoption_App_API.DevTools;
namespace Pawfect_Pet_Adoption_App_API.Models.Conversation
{
    public class ConversationValidator : AbstractValidator<ConversationPersist>
    {
        public ConversationValidator()
        {
            // Τα id των συμμετεχόντων είναι απαραίτητα
            RuleFor(conversation => conversation.UserIds)
                .Cascade(CascadeMode.Stop)
                .Must(users => users.All(RuleFluentValidation.IsObjectId))
                .WithMessage("Κάποιο από τα id των συμμετεχόντων δεν είναι σε σωστή μορφή.");

            // Το id του ζώου είναι απαραίτητο
            RuleFor(conversation => conversation.AnimalId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Το id του ζώου δεν είναι σε σωστή μορφή.");
        }
    }
}
