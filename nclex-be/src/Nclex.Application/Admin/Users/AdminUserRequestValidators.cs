using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.Users;

public sealed class ListAdminUsersRequestValidator : AbstractValidator<ListAdminUsersRequest>
{
    public ListAdminUsersRequestValidator()
    {
        RuleFor(r => r.Q).MaximumLength(ListAdminUsersRequest.MaxQueryLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.Limit).InclusiveBetween(1, AdminPaging.MaxLimit).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

public sealed class UpdateAdminUserRequestValidator : AbstractValidator<UpdateAdminUserRequest>
{
    public UpdateAdminUserRequestValidator()
    {
        RuleFor(r => r.Role).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.PlanTier).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
