using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.Signup;

public sealed class SignupRequestValidator : AbstractValidator<SignupRequest>
{
    public SignupRequestValidator()
    {
        RuleFor(r => r.Email).ValidEmail();
        RuleFor(r => r.Password).ValidNewPassword();
        RuleFor(r => r.Track)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.DisplayName).ValidDisplayName();
    }
}
