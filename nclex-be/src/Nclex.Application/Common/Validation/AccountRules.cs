using System.Net.Mail;
using FluentValidation;

namespace Nclex.Application.Common.Validation;

/// <summary>Field rules shared by sign-up and profile forms (api.md section 2).</summary>
public static class AccountRules
{
    public const int EmailMaxLength = 254;
    public const int PasswordMinLength = 10;
    public const int PasswordMaxLength = 128;
    public const int DisplayNameMaxLength = 80;

    /// <summary>Email: required, at most 254 characters, and a single valid address.</summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(EmailMaxLength).WithErrorCode(ValidationErrorCodes.TooLong)
            .Must(BeSingleEmailAddress).WithErrorCode(ValidationErrorCodes.InvalidFormat)
            .WithMessage("Enter a valid email address.");

    /// <summary>New password: 10 to 128 characters (architecture.md section 6).</summary>
    // TODO(F-later): also reject passwords on a breached-password list (architecture.md section 6).
    public static IRuleBuilderOptions<T, string> ValidNewPassword<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MinimumLength(PasswordMinLength).WithErrorCode(ValidationErrorCodes.TooShort)
            .MaximumLength(PasswordMaxLength).WithErrorCode(ValidationErrorCodes.TooLong);

    /// <summary>Display name: optional, at most 80 characters after trimming.</summary>
    public static IRuleBuilderOptions<T, string?> ValidDisplayName<T>(this IRuleBuilder<T, string?> rule) =>
        rule
            .Must(name => name is null || name.Trim().Length <= DisplayNameMaxLength)
            .WithErrorCode(ValidationErrorCodes.TooLong)
            .WithMessage($"Display name must be at most {DisplayNameMaxLength} characters.");

    /// <summary>Trims a display name; blank becomes null (no display name).</summary>
    public static string? NormalizeDisplayName(string? displayName) =>
        string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();

    // MailAddress also accepts "Name <a@b.com>" and lists; requiring the parsed address to equal
    // the input rejects those, leaving one plain address.
    private static bool BeSingleEmailAddress(string email) =>
        MailAddress.TryCreate(email, out var address) && address.Address == email;
}
