using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Bowtie;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>
/// Bowtie (interfaces.md 2.x). The test item has conditions c1 to c4, actions a1 to a5 and
/// parameters p1 to p5; the key is c2, a1 and a3, p2 and p4.
/// </summary>
public sealed class BowtieModuleTests
{
    private static readonly string[] AllIds = ["c1", "c2", "c3", "c4", "a1", "a2", "a3", "a4", "a5", "p1", "p2", "p3", "p4", "p5"];

    private readonly IItemTypeModule _module = new BowtieModule();

    // Authoring

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Content(), Key(), FullRationale()).ShouldBeEmpty();
    }

    [Fact]
    public void Max_points_is_always_five_one_per_slot()
    {
        _module.MaxPoints(Content(), Key()).ShouldBe(5);
    }

    [Fact]
    public void Each_list_needs_at_least_four_options()
    {
        var content = Content(conditions: 3);

        var issues = _module.ValidateAuthoring(content, Key(), RationaleFor(content));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("content.condition"),
            i => i.Code.ShouldBe(ValidationIssueCodes.TooFew));
    }

    [Fact]
    public void A_list_may_hold_at_most_eight_options()
    {
        var content = Content(actions: 9);

        var issues = _module.ValidateAuthoring(content, Key(), RationaleFor(content));

        issues.ShouldContain(i => i.Path == "content.actions" && i.Code == ValidationIssueCodes.TooMany);
    }

    [Fact]
    public void An_option_id_may_appear_in_only_one_list()
    {
        // The rationale is keyed by option id alone, so a shared id would get one explanation for two options.
        var content = Json(new
        {
            condition = Ids("c1", "c2", "c3", "a1"),
            actions = Ids("a1", "a2", "a3", "a4", "a5"),
            parameters = Ids("p1", "p2", "p3", "p4", "p5"),
        });

        var issues = _module.ValidateAuthoring(content, Key(), FullRationale());

        issues.ShouldContain(i => i.Path == "content" && i.Code == ValidationIssueCodes.DuplicateId && i.Message.Contains("\"a1\""));
    }

    [Fact]
    public void Each_wing_of_the_key_needs_exactly_two_options()
    {
        var issues = _module.ValidateAuthoring(Content(), Key(actions: ["a1"], parameters: ["p1", "p2", "p3"]), FullRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("key.actions", ValidationIssueCodes.Mismatch), ("key.parameters", ValidationIssueCodes.Mismatch)]);
    }

    [Fact]
    public void The_key_must_point_into_the_right_list()
    {
        // p1 is a parameter, so it cannot be the condition; a1 cannot be listed twice.
        var issues = _module.ValidateAuthoring(Content(), Key(condition: "p1", actions: ["a1", "a1"]), FullRationale());

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("key.condition[0]", ValidationIssueCodes.UnknownId), ("key.actions[1]", ValidationIssueCodes.DuplicateId)]);
    }

    [Fact]
    public void Every_option_in_every_list_needs_a_rationale()
    {
        var rationale = new Rationale("Why.", AllIds.Where(id => id != "p5").ToDictionary(id => id, id => $"Why {id}."));

        var issues = _module.ValidateAuthoring(Content(), Key(), rationale);

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("rationale.perOption.p5"),
            i => i.Code.ShouldBe(ValidationIssueCodes.MissingRationale));
    }

    [Fact]
    public void A_key_missing_a_wing_does_not_read()
    {
        var key = Json(new { condition = "c2", actions = new[] { "a1", "a3" } });

        _module.ValidateAuthoring(Content(), key, FullRationale())
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.InvalidShape);
    }

    // Responses

    [Fact]
    public void A_half_filled_bowtie_can_be_saved()
    {
        _module.ValidateResponse(Content(), Response(null, ["a1"], [])).ShouldBeEmpty();
    }

    [Fact]
    public void A_wing_holds_at_most_two_options()
    {
        var issues = _module.ValidateResponse(Content(), Response("c2", ["a1", "a2", "a3"], ["p1"]));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("response.actions"),
            i => i.Code.ShouldBe(ValidationIssueCodes.TooMany));
    }

    [Fact]
    public void A_response_must_place_options_in_their_own_list()
    {
        var issues = _module.ValidateResponse(Content(), Response("a1", ["p1"], ["p2", "p2"]));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("response.condition[0]", ValidationIssueCodes.UnknownId),
            ("response.actions[0]", ValidationIssueCodes.UnknownId),
            ("response.parameters[1]", ValidationIssueCodes.DuplicateId),
        ]);
    }

    // Grading

    [Fact]
    public void A_fully_correct_bowtie_earns_five_points()
    {
        var result = _module.Grade(Content(), Key(), Response("c2", ["a3", "a1"], ["p4", "p2"]));

        (result.Points, result.MaxPoints, result.IsCorrect).ShouldBe((5m, 5m, true));
    }

    [Fact]
    public void Each_correct_slot_earns_a_point_and_a_wrong_one_costs_nothing()
    {
        var result = _module.Grade(Content(), Key(), Response("c1", ["a1", "a2"], ["p2"]));

        result.Points.ShouldBe(2);
        result.IsCorrect.ShouldBeFalse();
        result.Detail.Where(d => d.Delta != 0 || d.Selected is not null || d.Missed).ShouldBe(
        [
            new GradeDetail("c1", 0, Selected: "c1"),
            new GradeDetail("c2", 0, Missed: true),
            new GradeDetail("a1", +1, Selected: "a1"),
            new GradeDetail("a2", 0, Selected: "a2"),
            new GradeDetail("a3", 0, Missed: true),
            new GradeDetail("p2", +1, Selected: "p2"),
            new GradeDetail("p4", 0, Missed: true),
        ]);
    }

    [Fact]
    public void The_detail_has_one_entry_per_option_in_content_order()
    {
        var result = _module.Grade(Content(), Key(), Response("c2", [], []));

        result.Detail.Select(d => d.Target).ShouldBe(AllIds);
    }

    [Fact]
    public void A_blank_response_scores_zero_and_marks_every_correct_option_missed()
    {
        var result = _module.Grade(Content(), Key(), null);

        result.Points.ShouldBe(0);
        result.Detail.Where(d => d.Missed).Select(d => d.Target).ShouldBe(["c2", "a1", "a3", "p2", "p4"]);
    }

    // Presenting

    [Fact]
    public void Present_shuffles_each_list_but_keeps_its_options()
    {
        var presented = _module.Present(Content(), "attempt-1:4").Deserialize<BowtieContent>(ItemJson.Options)!;
        var original = Content().Deserialize<BowtieContent>(ItemJson.Options)!;

        presented.Condition.OrderBy(o => o.Id).ShouldBe(original.Condition);
        presented.Actions.OrderBy(o => o.Id).ShouldBe(original.Actions);
        presented.Parameters.OrderBy(o => o.Id).ShouldBe(original.Parameters);
    }

    [Fact]
    public void Present_gives_the_same_order_for_the_same_seed()
    {
        _module.Present(Content(), "attempt-1:4").GetRawText().ShouldBe(_module.Present(Content(), "attempt-1:4").GetRawText());
    }

    [Fact]
    public void Shuffling_does_not_change_the_grade()
    {
        var response = Response("c2", ["a1", "a4"], ["p2"]);

        _module.Grade(_module.Present(Content(), "seed"), Key(), response).Points
            .ShouldBe(_module.Grade(Content(), Key(), response).Points);
    }

    private static JsonElement Content(int conditions = 4, int actions = 5, int parameters = 5) =>
        Json(new
        {
            condition = Numbered("c", conditions),
            actions = Numbered("a", actions),
            parameters = Numbered("p", parameters),
        });

    private static JsonElement Key(string condition = "c2", string[]? actions = null, string[]? parameters = null) =>
        Json(new { condition, actions = actions ?? ["a1", "a3"], parameters = parameters ?? ["p2", "p4"] });

    private static JsonElement Response(string? condition, string[] actions, string[] parameters) =>
        Json(new { condition, actions, parameters });

    private static Rationale FullRationale() => new("Why.", AllIds.ToDictionary(id => id, id => $"Why {id}."));

    private static Rationale RationaleFor(JsonElement content)
    {
        var ids = new[] { "condition", "actions", "parameters" }
            .SelectMany(list => content.GetProperty(list).EnumerateArray().Select(o => o.GetProperty("id").GetString()!));
        return new Rationale("Why.", ids.ToDictionary(id => id, id => $"Why {id}."));
    }

    private static object[] Numbered(string prefix, int count) =>
        Ids([.. Enumerable.Range(1, count).Select(i => $"{prefix}{i}")]);

    private static object[] Ids(params string[] ids) => [.. ids.Select(id => new { id, text = $"Option {id}" })];
}
