using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Sata;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class SataModuleTests
{
    private readonly IItemTypeModule _module = new SataModule();

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Json(Options(6)), SataKey("a", "c"), RationaleFor(6)).ShouldBeEmpty();
    }

    [Fact]
    public void An_option_without_a_rationale_is_rejected()
    {
        var issues = _module.ValidateAuthoring(Json(Options(5)), SataKey("a"), RationaleFor(4));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.e"),
            i => i.Code.ShouldBe(ValidationIssueCodes.MissingRationale));
    }

    [Theory]
    [InlineData(4, ValidationIssueCodes.TooFew)]
    [InlineData(11, ValidationIssueCodes.TooMany)]
    public void Needs_5_to_10_options(int optionCount, string code)
    {
        var issues = _module.ValidateAuthoring(Json(Options(optionCount)), SataKey("a"), RationaleFor(optionCount));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("content.options"),
            i => i.Code.ShouldBe(code));
    }

    [Fact]
    public void Needs_at_least_one_correct_option()
    {
        var issues = _module.ValidateAuthoring(Json(Options(5)), SataKey(), RationaleFor(5));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("key.correct"),
            i => i.Code.ShouldBe(ValidationIssueCodes.TooFew));
    }

    [Fact]
    public void Key_ids_must_be_options_and_unique()
    {
        var issues = _module.ValidateAuthoring(Json(Options(5)), SataKey("a", "a", "z"), RationaleFor(5));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("key.correct[1]", ValidationIssueCodes.DuplicateId), ("key.correct[2]", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void Max_points_is_the_number_of_correct_options()
    {
        _module.MaxPoints(Json(Options(6)), SataKey("a", "b", "e")).ShouldBe(3);
    }

    [Fact]
    public void Grade_detail_gives_plus_one_minus_one_and_missed()
    {
        var result = _module.Grade(Json(Options(5)), SataKey("a", "b"), SataResponse("a", "c"));

        result.Detail.ShouldBe(
        [
            new GradeDetail("a", +1, Selected: "a"),
            new GradeDetail("b", 0, Missed: true),
            new GradeDetail("c", -1, Selected: "c"),
            new GradeDetail("d", 0),
            new GradeDetail("e", 0),
        ]);
        result.Points.ShouldBe(0);
        result.IsCorrect.ShouldBeFalse();
    }

    [Fact]
    public void A_response_with_unknown_or_repeated_ids_is_rejected()
    {
        var issues = _module.ValidateResponse(Json(Options(5)), SataResponse("a", "a", "q"));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("response.selected[1]", ValidationIssueCodes.DuplicateId), ("response.selected[2]", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void A_response_that_is_not_an_object_is_reported()
    {
        _module.ValidateResponse(Json(Options(5)), Json("[\"a\"]")).ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.InvalidShape);
    }
}
