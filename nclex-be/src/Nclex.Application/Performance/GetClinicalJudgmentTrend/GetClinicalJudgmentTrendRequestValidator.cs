using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Performance.GetClinicalJudgmentTrend;

public sealed class GetClinicalJudgmentTrendRequestValidator : AbstractValidator<GetClinicalJudgmentTrendRequest>
{
    public GetClinicalJudgmentTrendRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
