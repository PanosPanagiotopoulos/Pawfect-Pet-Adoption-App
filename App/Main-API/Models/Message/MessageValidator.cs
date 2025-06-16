using FluentValidation;
using Main_API.DevTools;

namespace Main_API.Models.Message
{
    public class MessageValidator : AbstractValidator<MessagePersist>
    {
        public MessageValidator()
        {
            // The conversation ID is required
            RuleFor(message => message.ConversationId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The conversation ID is not in the correct format.");

            // The sender ID is required
            RuleFor(message => message.SenderId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The sender ID is not in the correct format.");

            // The recipient ID is required
            RuleFor(message => message.RecipientId)
                .Cascade(CascadeMode.Stop)
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The recipient ID is not in the correct format.");

            // The message content is required
            RuleFor(message => message.Content)
                .Cascade(CascadeMode.Stop)
                .Length(1, 1000)
                .WithMessage("The message content must be between 1 and 1000 characters.");
        }
    }
}