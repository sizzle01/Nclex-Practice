using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Media;

public sealed class UpdateMediaRequestValidator : AbstractValidator<UpdateMediaRequest>
{
    public const int MaxAltTextLength = 1000;

    public UpdateMediaRequestValidator()
    {
        RuleFor(r => r.AltText).MaximumLength(MaxAltTextLength).WithErrorCode(ValidationErrorCodes.TooLong);
        RuleFor(r => r.Captions)
            .MaximumLength(MediaUploadRules.MaxTextLength).WithErrorCode(ValidationErrorCodes.TooLong)
            .Must(text => text is null || MediaUploadRules.IsWebVtt(text)).WithErrorCode(ValidationErrorCodes.InvalidFormat);
        RuleFor(r => r.Transcript).MaximumLength(MediaUploadRules.MaxTextLength).WithErrorCode(ValidationErrorCodes.TooLong);
    }
}
