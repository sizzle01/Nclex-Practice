using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.WeakAreas.GetWeakAreas;

public sealed class GetWeakAreasRequestValidator : AbstractValidator<GetWeakAreasRequest>
{
    public GetWeakAreasRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
