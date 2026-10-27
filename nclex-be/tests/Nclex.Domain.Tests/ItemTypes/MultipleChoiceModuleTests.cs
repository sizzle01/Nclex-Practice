using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.MultipleChoice;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class MultipleChoiceModuleTests
{
    private readonly IItemTypeModule _module = new MultipleChoiceModule();

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "c" }), RationaleFor(4)).ShouldBeEmpty();
    }

    [Fact]
    public void An_option_without_a_rationale_is_rejected()
    {
        var rationale = RationaleFor(3); // a, b, c but not d

        var issues = _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "c" }), rationale);

        issues.ShouldHaveSingleItem().ShouldBe(new ValidationIssue(
            "rationale.perOption.d", ValidationIssueCodes.MissingRationale, "Option \"d\" needs a rationale."));
    }

    [Fact]
    public void A_blank_rationale_for_an_option_is_rejected()
    {
        var rationale = RationaleFor(4) with { PerOption = new Dictionary<string, string> { ["a"] = "A", ["b"] = "B", ["c"] = "C", ["d"] = " " } };

        var issues = _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "c" }), rationale);

        issues.ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.MissingRationale);
    }

    [Fact]
    public void A_rationale_for_an_option_that_does_not_exist_is_rejected()
    {
        var issues = _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "c" }), RationaleFor(5));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.e"),
            i => i.Code.ShouldBe(ValidationIssueCodes.UnknownId));
    }

    [Fact]
    public void A_missing_overall_explanation_is_rejected()
    {
        var issues = _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "c" }), RationaleFor(4) with { Correct = "" });

        issues.ShouldHaveSingleItem().Path.ShouldBe("rationale.correct");
    }

    [Fact]
    public void A_key_that_is_not_an_option_is_rejected()
    {
        var issues = _module.ValidateAuthoring(Json(Options(4)), Json(new { correct = "z" }), RationaleFor(4));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("key.correct"),
            i => i.Code.ShouldBe(ValidationIssueCodes.UnknownId));
    }

    [Fact]
    public void Duplicate_option_ids_are_rejected()
    {
        var content = Json("""{ "options": [ { "id": "a", "text": "One" }, { "id": "a", "text": "Two" } ] }""");

        var issues = _module.ValidateAuthoring(content, Json(new { correct = "a" }), RationaleFor(1));

        issues.ShouldContain(i => i.Path == "content.options[1].id" && i.Code == ValidationIssueCodes.DuplicateId);
    }

    [Fact]
    public void Fewer_than_two_options_are_rejected()
    {
        var issues = _module.ValidateAuthoring(Json(Options(1)), Json(new { correct = "a" }), RationaleFor(1));

        issues.ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.TooFew);
    }

    [Fact]
    public void Payloads_with_the_wrong_shape_are_reported_not_thrown()
    {
        var issues = _module.ValidateAuthoring(Json("""{ "choices": [] }"""), Json("[]"), RationaleFor(0));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("content", ValidationIssueCodes.InvalidShape), ("key", ValidationIssueCodes.InvalidShape)]);
    }

    [Fact]
    public void Max_points_is_one()
    {
        _module.MaxPoints(Json(Options(4)), Json(new { correct = "c" })).ShouldBe(1);
    }

    [Fact]
    public void Grade_detail_shows_the_choice_and_the_missed_answer()
    {
        var result = _module.Grade(Json(Options(4)), Json(new { correct = "c" }), Json(new { selected = "a" }));

        result.Detail.ShouldBe(
        [
            new GradeDetail("a", 0, Selected: "a"),
            new GradeDetail("b", 0),
            new GradeDetail("c", 0, Missed: true),
            new GradeDetail("d", 0),
        ]);
    }

    [Fact]
    public void A_response_choosing_an_unknown_option_is_rejected()
    {
        var issues = _module.ValidateResponse(Json(Options(4)), Json(new { selected = "z" }));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("response.selected[0]"),
            i => i.Code.ShouldBe(ValidationIssueCodes.UnknownId));
    }

    [Fact]
    public void A_response_with_nothing_selected_is_valid()
    {
        _module.ValidateResponse(Json(Options(4)), Json("""{ "selected": null }""")).ShouldBeEmpty();
    }
}
