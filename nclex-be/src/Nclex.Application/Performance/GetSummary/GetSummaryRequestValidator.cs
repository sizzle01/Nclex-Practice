using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Performance.GetSummary;

public sealed class GetSummaryRequestValidator : AbstractValidator<GetSummaryRequest>
{
    public GetSummaryRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
