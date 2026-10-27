using FluentValidation.TestHelper;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Application.Tests.Catalog;

public sealed class CreateCustomSetRequestValidatorTests
{
    private static readonly CreateCustomSetRequest Valid = new(
        ExamTrack.RN, [2, 7], [], [ItemType.Sata], [DifficultyTier.Moderate], PoolMode.Unused,
        QuestionCount: 30, TestMode.Practice, RevealMode.Tutor, new CustomSetTiming(true, null, 3600), StartNow: true);

    private readonly CreateCustomSetRequestValidator _validator = new();

    [Fact]
    public void The_api_md_example_is_valid() =>
        _validator.TestValidate(Valid).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Only_count_mode_and_reveal_mode_are_required()
    {
        var result = _validator.TestValidate(new CreateCustomSetRequest());

        result.ShouldHaveValidationErrorFor(r => r.QuestionCount).WithErrorCode("REQUIRED");
        result.ShouldHaveValidationErrorFor(r => r.Mode).WithErrorCode("REQUIRED");
        result.ShouldHaveValidationErrorFor(r => r.RevealMode).WithErrorCode("REQUIRED");
        result.ShouldNotHaveValidationErrorFor(r => r.Timing);
        result.ShouldNotHaveValidationErrorFor(r => r.Pool);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(151)]
    public void The_count_is_between_5_and_150(int count) =>
        _validator.TestValidate(Valid with { QuestionCount = count })
            .ShouldHaveValidationErrorFor(r => r.QuestionCount).WithErrorCode("INVALID_VALUE");

    [Theory]
    [InlineData(5)]
    [InlineData(150)]
    public void The_count_limits_are_inclusive(int count) =>
        _validator.TestValidate(Valid with { QuestionCount = count }).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Cat_mode_is_not_for_the_builder() =>
        _validator.TestValidate(Valid with { Mode = TestMode.Cat })
            .ShouldHaveValidationErrorFor(r => r.Mode).WithErrorCode("INVALID_VALUE");

    [Fact]
    public void A_timed_set_needs_a_timer() =>
        _validator.TestValidate(Valid with { Timing = new CustomSetTiming(Timed: true) })
            .ShouldHaveValidationErrorFor("Timing.Timed").WithErrorCode("INVALID_VALUE");

    [Fact]
    public void An_untimed_set_has_no_timers() =>
        _validator.TestValidate(Valid with { Timing = new CustomSetTiming(Timed: false, PerQuestionSec: 90) })
            .ShouldHaveValidationErrorFor("Timing.Timed").WithErrorCode("INVALID_VALUE");

    [Fact]
    public void Timers_are_positive() =>
        _validator.TestValidate(Valid with { Timing = new CustomSetTiming(true, PerQuestionSec: 0, TimeBudgetSec: -1) })
            .ShouldSatisfyAllConditions(
                r => r.ShouldHaveValidationErrorFor("Timing.PerQuestionSec").WithErrorCode("INVALID_VALUE"),
                r => r.ShouldHaveValidationErrorFor("Timing.TimeBudgetSec").WithErrorCode("INVALID_VALUE"));

    [Fact]
    public void Undefined_enum_values_in_filters_are_invalid()
    {
        var result = _validator.TestValidate(Valid with
        {
            Pool = (PoolMode)9,
            ItemTypes = [(ItemType)99],
            Difficulties = [(DifficultyTier)9],
        });

        result.ShouldHaveValidationErrorFor(r => r.Pool).WithErrorCode("INVALID_VALUE");
        result.ShouldHaveValidationErrorFor("ItemTypes[0]").WithErrorCode("INVALID_VALUE");
        result.ShouldHaveValidationErrorFor("Difficulties[0]").WithErrorCode("INVALID_VALUE");
    }

    [Fact]
    public void The_count_request_shares_the_filter_rules() =>
        new AvailableCountRequestValidator().TestValidate(new AvailableCountRequest(Pool: (PoolMode)9))
            .ShouldHaveValidationErrorFor(r => r.Pool).WithErrorCode("INVALID_VALUE");
}
