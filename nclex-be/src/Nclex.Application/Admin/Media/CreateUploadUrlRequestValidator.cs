using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Content;

namespace Nclex.Application.Admin.Media;

public sealed class CreateUploadUrlRequestValidator : AbstractValidator<CreateUploadUrlRequest>
{
    public CreateUploadUrlRequestValidator()
    {
        RuleFor(r => r.MimeType)
            .NotEmpty().WithErrorCode(ValidationErrorCodes.Required)
            .Must(mime => MediaUploadRules.TryGetKind(mime, out _)).WithErrorCode(ValidationErrorCodes.InvalidValue);
        RuleFor(r => r.Bytes)
            .InclusiveBetween(1, MediaUploadRules.MaxBytes).WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
