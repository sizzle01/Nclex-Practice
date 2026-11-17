using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.WeakAreas.StartWeakAreaPractice;

public sealed class StartWeakAreaPracticeRequestValidator : AbstractValidator<StartWeakAreaPracticeRequest>
{
    public StartWeakAreaPracticeRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
