using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Flags.Moderation;

public sealed class ListFlagQueueRequestValidator : AbstractValidator<ListFlagQueueRequest>
{
    public ListFlagQueueRequestValidator()
    {
        RuleFor(r => r.Status).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Limit)
            .InclusiveBetween(1, ListFlagQueueRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || FeedCursor.TryDecode(cursor, out _, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
