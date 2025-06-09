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
                .WithMessage("All users ids must be valid");

            // Το id του ζώου είναι απαραίτητο
            RuleFor(conversation => conversation.AnimalId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("Animal id not valid");
        }
    }
}
