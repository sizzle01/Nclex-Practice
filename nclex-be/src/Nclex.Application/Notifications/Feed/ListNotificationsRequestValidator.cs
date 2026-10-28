using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Notifications.Feed;

public sealed class ListNotificationsRequestValidator : AbstractValidator<ListNotificationsRequest>
{
    public ListNotificationsRequestValidator()
    {
        RuleFor(r => r.Limit)
            .InclusiveBetween(1, ListNotificationsRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || FeedCursor.TryDecode(cursor, out _, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
