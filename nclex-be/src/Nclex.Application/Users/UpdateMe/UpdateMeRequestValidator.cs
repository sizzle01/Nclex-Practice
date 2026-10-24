using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Users.UpdateMe;

public sealed class UpdateMeRequestValidator : AbstractValidator<UpdateMeRequest>
{
    public UpdateMeRequestValidator()
    {
        RuleFor(r => r.DisplayName).ValidDisplayName();
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Timezone)
            .Must(BeIanaTimeZone!)
            .When(r => r.Timezone is not null)
            .WithErrorCode(ValidationErrorCodes.InvalidValue)
            .WithMessage("Timezone must be an IANA time zone id, such as \"Europe/London\".");
    }

    // Accepts IANA ids only (not Windows ids such as "GMT Standard Time"), so stored values are
    // portable across servers.
    private static bool BeIanaTimeZone(string timezone) =>
        TimeZoneInfo.TryConvertIanaIdToWindowsId(timezone, out _);
}
