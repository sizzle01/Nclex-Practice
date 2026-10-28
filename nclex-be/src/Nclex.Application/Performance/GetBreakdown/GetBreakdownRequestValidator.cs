using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetBreakdown;

public sealed class GetBreakdownRequestValidator : AbstractValidator<GetBreakdownRequest>
{
    public GetBreakdownRequestValidator()
    {
        RuleFor(r => r.Dimension).NotNull().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.Dimension)
            .Must(d => d is not PerformanceDimension.Overall)
            .WithErrorCode(ValidationErrorCodes.InvalidValue)
            .WithMessage("Use GET /me/performance/summary for the overall totals.");
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
