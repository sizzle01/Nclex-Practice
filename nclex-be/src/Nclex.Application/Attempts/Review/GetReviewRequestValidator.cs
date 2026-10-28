using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.Review;

public sealed class GetReviewRequestValidator : AbstractValidator<GetReviewRequest>
{
    public GetReviewRequestValidator()
    {
        RuleFor(r => r.Position).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Limit)
            .InclusiveBetween(1, GetReviewRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
