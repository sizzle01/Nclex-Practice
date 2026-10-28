using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.Banks;

public sealed class CreateBankRequestValidator : AbstractValidator<CreateBankRequest>
{
    public CreateBankRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Name)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(BankLimits.MaxNameLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.Description).MaximumLength(BankLimits.MaxDescriptionLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.AccessTier).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

public sealed class UpdateBankRequestValidator : AbstractValidator<UpdateBankRequest>
{
    public UpdateBankRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(BankLimits.MaxNameLength).WithErrorCode(ValidationErrorCodes.TooLong)
            .When(r => r.Name is not null);
        RuleFor(r => r.Description).MaximumLength(BankLimits.MaxDescriptionLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.AccessTier).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

public sealed class ListAdminBanksRequestValidator : AbstractValidator<ListAdminBanksRequest>
{
    public ListAdminBanksRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Limit).InclusiveBetween(1, AdminPaging.MaxLimit).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

/// <summary>Length limits for bank text.</summary>
public static class BankLimits
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 2000;
}
