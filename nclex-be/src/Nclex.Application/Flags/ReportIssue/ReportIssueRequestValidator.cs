using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.ReportIssue;

public sealed class ReportIssueRequestValidator : AbstractValidator<ReportIssueRequest>
{
    public ReportIssueRequestValidator()
    {
        RuleFor(r => r.Reason).NotNull().WithErrorCode(ValidationErrorCodes.Required);
        RuleFor(r => r.Reason).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);

        // The trimmed length, because that is what is stored (QuestionFlag.Report).
        RuleFor(r => r.Comment)
            .Must(c => c is null || c.Trim().Length <= QuestionFlag.MaxCommentLength)
            .WithErrorCode(ValidationErrorCodes.TooLong);
    }
}
