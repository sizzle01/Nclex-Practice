using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.ConfirmEmailChange;

public sealed class ConfirmEmailChangeRequestValidator : AbstractValidator<ConfirmEmailChangeRequest>
{
    public ConfirmEmailChangeRequestValidator()
    {
        RuleFor(r => r.Token).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
