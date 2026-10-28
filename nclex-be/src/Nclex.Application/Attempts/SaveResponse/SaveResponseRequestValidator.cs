using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.SaveResponse;

public sealed class SaveResponseRequestValidator : AbstractValidator<SaveResponseRequest>
{
    public SaveResponseRequestValidator()
    {
        // Stored values start at 0, so the first real write is 1.
        RuleFor(r => r.ClientSeq).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
