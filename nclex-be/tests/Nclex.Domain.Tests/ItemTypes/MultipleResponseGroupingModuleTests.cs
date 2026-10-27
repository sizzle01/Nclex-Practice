using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.Grouped;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class MultipleResponseGroupingModuleTests
{
    private readonly IItemTypeModule _module = new MultipleResponseGroupingModule();

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Content(("g1", ["a", "b", "c"]), ("g2", ["d", "e"])), Key(("g1", ["a"]), ("g2", ["e"])), RationaleFor(5))
            .ShouldBeEmpty();
    }

    [Fact]
    public void Option_ids_must_be_unique_across_groups_because_rationales_are_keyed_by_option_id()
    {
        var issues = _module.ValidateAuthoring(Content(("g1", ["a", "b"]), ("g2", ["b", "c"])), Key(("g1", ["a"]), ("g2", ["c"])), RationaleFor(3));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("content.groups[1].options[0].id"),
            i => i.Code.ShouldBe(ValidationIssueCodes.DuplicateId));
    }

    [Fact]
    public void Each_group_needs_two_options_a_label_and_a_correct_option()
    {
        var content = Json(new { groups = new[] { new { id = "g1", label = " ", options = new[] { new { id = "a", text = "A" } } } } });

        var issues = _module.ValidateAuthoring(content, Json(new { byGroup = new Dictionary<string, string[]>() }), RationaleFor(1));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("content.groups[0].label", ValidationIssueCodes.Required),
            ("content.groups[0].options", ValidationIssueCodes.TooFew),
            ("key.byGroup.g1", ValidationIssueCodes.TooFew),
        ]);
    }

    [Fact]
    public void A_response_cannot_pick_an_option_from_another_group()
    {
        var issues = _module.ValidateResponse(Content(("g1", ["a", "b"]), ("g2", ["c", "d"])), Key(("g1", ["c"])));

        issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("response.byGroup.g1[0]"),
            i => i.Code.ShouldBe(ValidationIssueCodes.UnknownId));
    }

    [Fact]
    public void Present_shuffles_options_within_each_group_and_keeps_the_groups_in_order()
    {
        var content = Content(("g1", ["a", "b", "c", "d", "e", "f"]), ("g2", ["g", "h", "i", "j", "k", "l"]));

        var presented = _module.Present(content, "attempt-1:4").Deserialize<GroupingContent>(ItemJson.Options)!;

        presented.Groups.Select(g => g.Id).ShouldBe(["g1", "g2"]);
        presented.Groups[0].Options.Select(o => o.Id).ShouldBe(["a", "b", "c", "d", "e", "f"], ignoreOrder: true);
        presented.Groups[1].Options.Select(o => o.Id).ShouldBe(["g", "h", "i", "j", "k", "l"], ignoreOrder: true);
        presented.Groups.SelectMany(g => g.Options).Select(o => o.Id)
            .ShouldNotBe(["a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l"]);
    }

    private static JsonElement Content(params (string Id, string[] Options)[] groups) =>
        Json(new
        {
            groups = groups.Select(g => new { id = g.Id, label = $"Group {g.Id}", options = g.Options.Select(o => new { id = o, text = $"Option {o}" }) }),
        });

    private static JsonElement Key(params (string Group, string[] Ids)[] byGroup) =>
        Json(new { byGroup = byGroup.ToDictionary(p => p.Group, p => p.Ids) });
}
