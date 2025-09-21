using FluentValidation;
using Pawfect_API.DevTools;

namespace Pawfect_API.Models.AiAssistant
{
    public class CompletionsRequestValidator : AbstractValidator<CompletionsRequest>
    {
        public CompletionsRequestValidator()
        {
            RuleFor(request => request.Prompt)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("Prompt Is Required")
                .MinimumLength(2)
                .WithMessage("Prompt must have at least 2 characters");

            When(request => !String.IsNullOrEmpty(request.ContextAnimalId), () =>
            {
                RuleFor(request => request.ContextAnimalId)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("ContextAnimalId must be a valid ObjectId.");
            });

            When(request => request.ConversationHistory != null, () =>
            {
                RuleForEach(request => request.ConversationHistory)
                .Must(message => message.Role != default && !String.IsNullOrWhiteSpace(message.Content))
                .WithMessage("Messages must have valid roles and content");
            });
        }
    }
}
