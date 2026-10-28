using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

public sealed class UpdateFlagRequestValidator : AbstractValidator<UpdateFlagRequest>
{
    public UpdateFlagRequestValidator()
    {
        RuleFor(r => r.Status).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);

        // Open is only where a report starts (QuestionFlag.CanMove).
        RuleFor(r => r.Status).NotEqual(FlagStatus.Open).WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.ResolutionNote)
            .Must(note => !string.IsNullOrWhiteSpace(note))
            .When(r => r.Closes)
            .WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.ResolutionNote)
            .Must(note => note is null || note.Trim().Length <= UpdateFlagRequest.MaxNoteLength)
            .WithErrorCode(ValidationErrorCodes.TooLong);

        // An empty PATCH would change nothing, which is more likely a client bug than intended.
        RuleFor(r => r)
            .Must(r => r.Status is not null || r.AssigneeId is not null)
            .WithName("status")
            .WithErrorCode(ValidationErrorCodes.Required);
    }
}
