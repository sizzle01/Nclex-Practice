using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications.Preferences;

public sealed class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        RuleFor(r => r.Preferences).NotNull().WithErrorCode(ValidationErrorCodes.Required);

        // "Replace preferences" (api.md section 6): the body must name every category once.
        RuleFor(r => r.Preferences)
            .Must(EveryCategoryOnce)
            .When(r => r.Preferences is not null)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleForEach(r => r.Preferences).ChildRules(preference =>
        {
            preference.RuleFor(p => p.Category).NotNull().WithErrorCode(ValidationErrorCodes.Required);
            preference.RuleFor(p => p.Category).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
            preference.RuleFor(p => p.InApp).NotNull().WithErrorCode(ValidationErrorCodes.Required);
            preference.RuleFor(p => p.Email).NotNull().WithErrorCode(ValidationErrorCodes.Required);

            // Security notices cannot be switched off (architecture.md 5.6).
            preference.RuleFor(p => p.InApp)
                .Equal(true)
                .When(p => p.Category is { } category && NotificationDefaults.IsLocked(category))
                .WithErrorCode(ValidationErrorCodes.InvalidValue);
            preference.RuleFor(p => p.Email)
                .Equal(true)
                .When(p => p.Category is { } category && NotificationDefaults.IsLocked(category))
                .WithErrorCode(ValidationErrorCodes.InvalidValue);
        });
    }

    private static bool EveryCategoryOnce(IReadOnlyList<PreferenceInput>? preferences) =>
        preferences is not null
        && preferences.Count == Enum.GetValues<NotificationCategory>().Length
        && preferences.Select(p => p.Category).Distinct().Count() == preferences.Count;
}
