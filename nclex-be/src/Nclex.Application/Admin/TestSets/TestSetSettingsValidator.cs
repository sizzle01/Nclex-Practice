using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.TestSets;

/// <summary>
/// The rules the importer applies to a curated set's settings (F06), for sets made in the console:
/// practice or challenge only (CAT sets are generated), consistent timers, and a pass threshold that
/// is a share of points.
/// </summary>
public sealed class TestSetSettingsValidator : AbstractValidator<TestSetSettings>
{
    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 2000;

    public TestSetSettingsValidator()
    {
        RuleFor(s => s.Title)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(MaxTitleLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(s => s.Description).MaximumLength(MaxDescriptionLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(s => s.Mode)
            .Must(mode => mode is TestMode.Practice or TestMode.Challenge)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.RevealMode).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.AccessTier).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.Timed)
            .Must((settings, _) => settings.HasConsistentTiming)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.PerQuestionSec).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.TimeBudgetSec).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.PassThreshold)
            .Must(t => t is null or > 0 and <= 1)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(s => s.PausePolicy).NotNull().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(s => s.PausePolicy.MaxPauses).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue).When(s => s.PausePolicy is not null);
        RuleFor(s => s.PausePolicy.MaxPauseSec).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue).When(s => s.PausePolicy is not null);
    }
}
