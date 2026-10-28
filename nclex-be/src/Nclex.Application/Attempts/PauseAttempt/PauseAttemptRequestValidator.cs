using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.PauseAttempt;

public sealed class PauseAttemptRequestValidator : AbstractValidator<PauseAttemptRequest>
{
    public PauseAttemptRequestValidator()
    {
        RuleFor(r => r.Reason).NotNull().WithErrorCode(ValidationErrorCodes.Required);
    }
}
