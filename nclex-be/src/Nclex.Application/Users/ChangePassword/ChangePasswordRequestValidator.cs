using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Users.ChangePassword;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(r => r.CurrentPassword).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.NewPassword).ValidNewPassword();
    }
}
