using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.Navigate;

public sealed class NavigateRequestValidator : AbstractValidator<NavigateRequest>
{
    public NavigateRequestValidator()
    {
        // The upper bound depends on the attempt, so the handler checks it.
        RuleFor(r => r.ToPosition).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
