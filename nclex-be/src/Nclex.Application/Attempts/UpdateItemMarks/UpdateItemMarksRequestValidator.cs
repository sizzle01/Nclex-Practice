using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Attempts.UpdateItemMarks;

public sealed class UpdateItemMarksRequestValidator : AbstractValidator<UpdateItemMarksRequest>
{
    public UpdateItemMarksRequestValidator()
    {
        RuleFor(r => r.StruckOptions)
            .Must(ids => ids is null || ids.Count <= UpdateItemMarksRequest.MaxStruckOptions)
            .WithErrorCode(ValidationErrorCodes.TooLong);

        RuleForEach(r => r.StruckOptions)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .MaximumLength(UpdateItemMarksRequest.MaxOptionIdLength).WithErrorCode(ValidationErrorCodes.TooLong);
    }
}
