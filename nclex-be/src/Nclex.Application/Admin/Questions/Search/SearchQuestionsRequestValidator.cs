using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.Questions.Search;

public sealed class SearchQuestionsRequestValidator : AbstractValidator<SearchQuestionsRequest>
{
    public SearchQuestionsRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Status).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.ItemType).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.QuestionStatus).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Q).MaximumLength(SearchQuestionsRequest.MaxQueryLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.Limit).InclusiveBetween(1, AdminPaging.MaxLimit).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
