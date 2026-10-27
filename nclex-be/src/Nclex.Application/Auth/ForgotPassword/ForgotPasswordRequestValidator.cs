using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.ForgotPassword;

/// <summary>Checks the email's format only; whether an account exists is never revealed.</summary>
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(r => r.Email).ValidEmail();
    }
}
