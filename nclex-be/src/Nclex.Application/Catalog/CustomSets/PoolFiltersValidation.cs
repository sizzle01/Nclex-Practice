using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Catalog.CustomSets;

/// <summary>Rules every request with builder filters shares. Whether ids exist is checked by <see cref="QuestionPool"/>.</summary>
internal static class PoolFiltersValidation
{
    public static void AddPoolFilterRules<T>(this AbstractValidator<T> validator)
        where T : IPoolFilters
    {
        validator.RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        validator.RuleFor(r => r.Pool).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        validator.RuleForEach(r => r.ItemTypes).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
        validator.RuleForEach(r => r.Difficulties).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}

public sealed class AvailableCountRequestValidator : AbstractValidator<AvailableCountRequest>
{
    public AvailableCountRequestValidator()
    {
        this.AddPoolFilterRules();
    }
}
