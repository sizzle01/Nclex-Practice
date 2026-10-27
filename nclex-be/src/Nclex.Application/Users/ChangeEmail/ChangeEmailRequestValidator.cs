using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Users.ChangeEmail;

public sealed class ChangeEmailRequestValidator : AbstractValidator<ChangeEmailRequest>
{
    public ChangeEmailRequestValidator()
    {
        RuleFor(r => r.CurrentPassword).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.NewEmail).ValidEmail();
    }
}
