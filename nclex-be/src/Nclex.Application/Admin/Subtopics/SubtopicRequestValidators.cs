using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Recommendations;

namespace Nclex.Application.Admin.Subtopics;

public sealed class CreateSubtopicRequestValidator : AbstractValidator<CreateSubtopicRequest>
{
    public CreateSubtopicRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(SubtopicNames.MaxNameLength).WithErrorCode(ValidationErrorCodes.TooLong)
            .Must(name => SubtopicNames.Slug(name ?? string.Empty).Length > 0).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

public sealed class UpdateSubtopicRequestValidator : AbstractValidator<UpdateSubtopicRequest>
{
    public UpdateSubtopicRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(SubtopicNames.MaxNameLength).WithErrorCode(ValidationErrorCodes.TooLong)
            .When(r => r.Name is not null);
    }
}

public sealed class SetQuestionSubtopicsRequestValidator : AbstractValidator<SetQuestionSubtopicsRequest>
{
    public SetQuestionSubtopicsRequestValidator()
    {
        RuleFor(r => r.SubtopicIds)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .Must(ids => ids is { Count: >= SubtopicNames.MinPerQuestion and <= SubtopicNames.MaxPerQuestion }).WithErrorCode(ValidationErrorCodes.InvalidValue)
            .Must(ids => ids is null || ids.Distinct().Count() == ids.Count).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
