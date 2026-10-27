using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.ResetPassword;

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(r => r.Token).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.NewPassword).ValidNewPassword();
    }
}
