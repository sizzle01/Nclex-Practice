using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.SubmitAttempt;

public sealed class SubmitAttemptRequestValidator : AbstractValidator<SubmitAttemptRequest>
{
    /// <summary>A flush holds at most one answer per item; this is far above any set's size.</summary>
    public const int MaxFinalResponses = 300;

    public SubmitAttemptRequestValidator()
    {
        RuleFor(r => r.FinalResponses)
            .Must(responses => responses is null || responses.Count <= MaxFinalResponses)
            .WithErrorCode(ValidationErrorCodes.TooLong);

        RuleForEach(r => r.FinalResponses).ChildRules(response =>
        {
            response.RuleFor(r => r.Position).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
            // As in autosave: stored values start at 0, so the first real write is 1.
            response.RuleFor(r => r.ClientSeq).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
        });
    }
}
