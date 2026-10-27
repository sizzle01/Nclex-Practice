using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Users.DeleteAccount;

public sealed class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(r => r.CurrentPassword).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}
