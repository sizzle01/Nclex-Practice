using FluentValidation;
using Nclex.Application.Catalog;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.TestSets;

public sealed class CreateTestSetRequestValidator : AbstractValidator<CreateTestSetRequest>
{
    public CreateTestSetRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Settings).NotNull().WithErrorCode(ValidationErrorCodes.Required).SetValidator(new TestSetSettingsValidator());
        RuleFor(r => r.QuestionIds).NotEmpty().WithErrorCode(ValidationErrorCodes.Required);
    }
}

public sealed class UpdateTestSetRequestValidator : AbstractValidator<UpdateTestSetRequest>
{
    public UpdateTestSetRequestValidator()
    {
        RuleFor(r => r.Settings).SetValidator(new TestSetSettingsValidator()!).When(r => r.Settings is not null);
        RuleFor(r => r.QuestionIds).NotEmpty().WithErrorCode(ValidationErrorCodes.Required).When(r => r.QuestionIds is not null);
    }
}

public sealed class ListAdminTestSetsRequestValidator : AbstractValidator<ListAdminTestSetsRequest>
{
    public ListAdminTestSetsRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Limit).InclusiveBetween(1, AdminPaging.MaxLimit).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Cursor)
            .Must(cursor => cursor is null || PageCursor.TryDecode(cursor, out _))
            .WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
