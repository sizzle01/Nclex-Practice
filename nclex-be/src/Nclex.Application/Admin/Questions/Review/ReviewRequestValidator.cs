using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.Questions.Review;

/// <summary>Approve requests: notes are optional but bounded.</summary>
public sealed class ReviewRequestValidator : AbstractValidator<ReviewRequest>
{
    public ReviewRequestValidator()
    {
        RuleFor(r => r.Notes).MaximumLength(ReviewRequest.MaxNotesLength).WithErrorCode(ValidationErrorCodes.TooLong);
    }
}
