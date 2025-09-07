using FluentValidation;
using Pawfect_Messenger.Data.Entities.EnumTypes;
using Pawfect_Messenger.DevTools;

namespace Pawfect_Messenger.Models.Message
{
    public class MessageValidator : AbstractValidator<MessagePersist>
    {
        public MessageValidator()
        {
            // The ID is optional for new messages but must be valid ObjectId if provided
            When(message => !String.IsNullOrEmpty(message.Id), () =>
            {
                RuleFor(message => message.Id)
                    .Cascade(CascadeMode.Stop)
                    .Must(RuleFluentValidation.IsObjectId)
                    .WithMessage("The message ID is not in the correct format.");
            });

            // The conversation ID is required
            RuleFor(message => message.ConversationId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("The conversation ID is required.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The conversation ID is not in the correct format.");

            // The sender ID is required
            RuleFor(message => message.SenderId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("The sender ID is required.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The sender ID is not in the correct format.");

            // The message type is required and must be valid enum
            RuleFor(message => message.Type)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The message type is not valid.")
                .IsInEnum()
                .WithMessage("The message type is not valid.");

            // The message content is required based on type
            When(message => message.Type == MessageType.Text, () =>
            {
                RuleFor(message => message.Content)
                    .Cascade(CascadeMode.Stop)
                    .NotEmpty()
                    .WithMessage("The message content is required for text messages.")
                    .Length(1, 5000)
                    .WithMessage("The message content must be between 1 and 5000 characters.");
            });

            // The message status is required and must be valid enum
            RuleFor(message => message.Status)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The message status is not valid.")
                .IsInEnum()
                .WithMessage("The message status is not valid.");
        }
    }

    public class MessageReadPersistValidator : AbstractValidator<MessageReadPersist>
    {
        public MessageReadPersistValidator()
        {
            // The message ID is required
            RuleFor(read => read.MessageId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("The message ID is required.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The message ID is not in the correct format.");

            // The user IDs list is required and must not be empty
            RuleFor(read => read.UserIds)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage("The user IDs list is required.")
                .NotEmpty()
                .WithMessage("At least one user ID must be provided.")
                .Must(userIds => userIds != null && userIds.Count <= 100)
                .WithMessage("Cannot mark message as read for more than 100 users at once.");

            // Each user ID must be valid ObjectId
            RuleForEach(read => read.UserIds)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("User ID cannot be empty.")
                .Must(RuleFluentValidation.IsObjectId)
                .WithMessage("The user ID '{PropertyValue}' is not in the correct format.");
        }
    }
}