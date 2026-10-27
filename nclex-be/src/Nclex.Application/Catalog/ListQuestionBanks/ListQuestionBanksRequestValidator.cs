using FluentValidation;
using Nclex.Application.Common.Validation;

namespace Nclex.Application.Catalog.ListQuestionBanks;

public sealed class ListQuestionBanksRequestValidator : AbstractValidator<ListQuestionBanksRequest>
{
    public ListQuestionBanksRequestValidator()
    {
        RuleFor(r => r.Track).IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);
    }
}
