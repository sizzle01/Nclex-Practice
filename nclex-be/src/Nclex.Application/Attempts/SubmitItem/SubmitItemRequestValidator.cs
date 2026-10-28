using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.SubmitItem;

public sealed class SubmitItemRequestValidator : AbstractValidator<SubmitItemRequest>
{
    public SubmitItemRequestValidator()
    {
        // As in autosave: stored values start at 0, so the first real write is 1.
        RuleFor(r => r.ClientSeq).GreaterThan(0).When(r => r.ClientSeq is not null).WithErrorCode(ValidationErrorCodes.InvalidValue);

        // Without a clientSeq the answer could not be ordered against autosaves still in flight.
        RuleFor(r => r.ClientSeq).NotNull().When(r => r.Response is not null).WithErrorCode(ValidationErrorCodes.Required);
    }
}
