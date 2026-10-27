using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.StartAttempt;

public sealed class StartAttemptRequestValidator : AbstractValidator<StartAttemptRequest>
{
    public StartAttemptRequestValidator()
    {
        RuleFor(r => r.TestSetId).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
