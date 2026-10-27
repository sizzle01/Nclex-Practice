using FluentValidation;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog.CustomSets;

public sealed class CreateCustomSetRequestValidator : AbstractValidator<CreateCustomSetRequest>
{
    public CreateCustomSetRequestValidator()
    {
        this.AddPoolFilterRules();

        RuleFor(r => r.QuestionCount)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .InclusiveBetween(CreateCustomSetRequest.MinQuestions, CreateCustomSetRequest.MaxQuestions)
            .WithErrorCode(ValidationErrorCodes.InvalidValue);

        // CAT sets are adaptive and built during the attempt (phase 2), not by the builder.
        RuleFor(r => r.Mode)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .Must(mode => mode is TestMode.Practice or TestMode.Challenge).WithErrorCode(ValidationErrorCodes.InvalidValue);

        RuleFor(r => r.RevealMode)
            .NotNull().WithErrorCode(ValidationErrorCodes.Required)
            .IsInEnum().WithErrorCode(ValidationErrorCodes.InvalidValue);

        When(r => r.Timing is not null, () =>
        {
            RuleFor(r => r.Timing!.PerQuestionSec).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);
            RuleFor(r => r.Timing!.TimeBudgetSec).GreaterThan(0).WithErrorCode(ValidationErrorCodes.InvalidValue);

            // The DDL CHECK: a timed set needs a timer. An untimed one must not carry any.
            RuleFor(r => r.Timing!.Timed)
                .Must((request, timed) => timed
                    ? request.Timing!.PerQuestionSec is not null || request.Timing.TimeBudgetSec is not null
                    : request.Timing!.PerQuestionSec is null && request.Timing.TimeBudgetSec is null)
                .WithErrorCode(ValidationErrorCodes.InvalidValue);
        });
    }
}
