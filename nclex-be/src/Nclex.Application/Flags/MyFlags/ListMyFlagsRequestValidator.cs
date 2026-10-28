using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Application.Notifications.Feed;

namespace Nclex.Application.Flags.MyFlags;

public sealed class ListMyFlagsRequestValidator : AbstractValidator<ListMyFlagsRequest>
{
    public ListMyFlagsRequestValidator()
    {
        RuleFor(r => r.Limit)
            .InclusiveBetween(1, ListMyFlagsRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || FeedCursor.TryDecode(cursor, out _, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
