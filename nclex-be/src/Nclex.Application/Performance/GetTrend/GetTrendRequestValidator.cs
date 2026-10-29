using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Performance;

namespace Nclex.Application.Performance.GetTrend;

public sealed class GetTrendRequestValidator : AbstractValidator<GetTrendRequest>
{
    public GetTrendRequestValidator()
    {
        RuleFor(r => r.Dimension).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Key)
            .NotEmpty()
            .When(r => r.Dimension is not null and not PerformanceDimension.Overall)
            .WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.Bucket).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.To)
            .GreaterThanOrEqualTo(r => r.From)
            .When(r => r.From is not null && r.To is not null)
            .WithErrorCode(ValidationErrorCodes.InvalidValue)
            .WithMessage("'to' must be on or after 'from'.");
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
