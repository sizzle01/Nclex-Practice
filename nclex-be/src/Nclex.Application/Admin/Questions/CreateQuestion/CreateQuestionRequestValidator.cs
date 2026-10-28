using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Admin.Questions.CreateQuestion;

public sealed class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.AccessTier).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Draft).NotNull().WithErrorCode(ValidationErrorCodes.Required);
    }
}
