using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.ListAttempts;

public sealed class ListAttemptsRequestValidator : AbstractValidator<ListAttemptsRequest>
{
    public ListAttemptsRequestValidator()
    {
        RuleFor(r => r.Status)
            .Must(status => AttemptStatusFilter.TryParse(status, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Limit)
            .InclusiveBetween(1, ListAttemptsRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
