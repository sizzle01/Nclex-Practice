using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.SelectN;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class SelectNModuleTests
{
    private readonly IItemTypeModule _module = new SelectNModule();

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Content(6, selectCount: 3), Key("a", "c", "e"), RationaleFor(6)).ShouldBeEmpty();
    }

    [Fact]
    public void The_key_must_list_exactly_select_count_options()
    {
        var issues = _module.ValidateAuthoring(Content(6, selectCount: 3), Key("a", "c"), RationaleFor(6));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("key.correct"),
            i => i.Code.ShouldBe(ValidationIssueCodes.Mismatch));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void The_select_count_must_be_between_1_and_fewer_than_the_options(int selectCount)
    {
        var key = Key([.. Enumerable.Range(0, selectCount).Select(OptionId)]);

        var issues = _module.ValidateAuthoring(Content(6, selectCount), key, RationaleFor(6));

        issues.ShouldContain(i => i.Path == "content.selectCount" && i.Code == ValidationIssueCodes.InvalidValue);
    }

    [Fact]
    public void An_unknown_scoring_override_is_reported()
    {
        var key = Json(new { correct = new[] { "a" }, scoringOverride = "half_marks" });

        _module.ValidateAuthoring(Content(4, selectCount: 1), key, RationaleFor(4))
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.InvalidShape);
    }

    [Fact]
    public void Max_points_is_the_select_count_or_1_with_the_all_or_nothing_override()
    {
        _module.MaxPoints(Content(6, selectCount: 3), Key("a", "c", "e")).ShouldBe(3);
        _module.MaxPoints(Content(6, selectCount: 3), Json(new { correct = new[] { "a", "c", "e" }, scoringOverride = "all_or_nothing" })).ShouldBe(1);
    }

    [Fact]
    public void Selecting_more_than_n_options_is_rejected()
    {
        var issues = _module.ValidateResponse(Content(6, selectCount: 2), Json(new { selected = new[] { "a", "b", "c" } }));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("response.selected"),
            i => i.Code.ShouldBe(ValidationIssueCodes.TooMany));
    }

    [Fact]
    public void Selecting_up_to_n_options_is_fine()
    {
        _module.ValidateResponse(Content(6, selectCount: 2), Json(new { selected = new[] { "a" } })).ShouldBeEmpty();
    }

    [Fact]
    public void A_response_with_unknown_or_repeated_ids_is_rejected()
    {
        var issues = _module.ValidateResponse(Content(6, selectCount: 3), Json(new { selected = new[] { "a", "a", "z" } }));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("response.selected[1]", ValidationIssueCodes.DuplicateId), ("response.selected[2]", ValidationIssueCodes.UnknownId)]);
    }

    [Fact]
    public void A_wrong_selection_shows_in_the_detail_without_losing_a_point()
    {
        var result = _module.Grade(Content(4, selectCount: 2), Key("a", "b"), Json(new { selected = new[] { "a", "c" } }));

        result.Detail.ShouldBe(
        [
            new GradeDetail("a", +1, Selected: "a"),
            new GradeDetail("b", 0, Missed: true),
            new GradeDetail("c", 0, Selected: "c"),
            new GradeDetail("d", 0),
        ]);
        result.Points.ShouldBe(1);
    }

    private static System.Text.Json.JsonElement Content(int optionCount, int selectCount) =>
        Json(new
        {
            options = Enumerable.Range(0, optionCount).Select(i => new { id = OptionId(i), text = $"Option {OptionId(i)}" }),
            selectCount,
        });

    private static System.Text.Json.JsonElement Key(params string[] correct) => Json(new { correct });
}
