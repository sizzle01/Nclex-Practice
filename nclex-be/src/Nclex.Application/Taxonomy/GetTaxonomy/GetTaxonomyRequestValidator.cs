using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Taxonomy.GetTaxonomy;

public sealed class GetTaxonomyRequestValidator : AbstractValidator<GetTaxonomyRequest>
{
    public GetTaxonomyRequestValidator()
    {
        RuleFor(r => r.Track)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
