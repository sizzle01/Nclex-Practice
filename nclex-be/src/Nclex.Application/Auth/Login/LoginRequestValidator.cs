using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.Login;

/// <summary>
/// Only checks that both fields are present. Length and format rules are for new passwords; an
/// existing account's password is simply checked against its hash.
/// </summary>
public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.Password).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
