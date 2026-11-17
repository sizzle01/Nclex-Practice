using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.ListTestSets;

public sealed class ListTestSetsRequestValidator : AbstractValidator<ListTestSetsRequest>
{
    public ListTestSetsRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);

        // Custom sets have their own lists (F07); assigned sets arrive with phase 3.
        RuleFor(r => r.Kind)
            .Must(kind => kind is null or TestSetKind.Prebuilt or TestSetKind.Cat)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Limit)
            .InclusiveBetween(1, ListTestSetsRequest.MaxLimit)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
