using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Auth.VerifyEmail;

public sealed class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(r => r.Token).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
