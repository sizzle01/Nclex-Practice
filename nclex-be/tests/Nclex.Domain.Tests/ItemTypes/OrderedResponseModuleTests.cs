using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.OrderedResponse;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class OrderedResponseModuleTests
{
    private readonly IItemTypeModule _module = new OrderedResponseModule();

    [Fact]
    public void A_complete_item_has_no_authoring_issues()
    {
        _module.ValidateAuthoring(Items(4), Key("all_or_nothing", "c", "a", "d", "b"), RationaleFor(4)).ShouldBeEmpty();
    }

    [Fact]
    public void The_key_must_be_a_permutation_of_the_items()
    {
        var issues = _module.ValidateAuthoring(Items(4), Key("per_position", "a", "a", "z"), RationaleFor(4));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("key.order[1]", ValidationIssueCodes.DuplicateId),
            ("key.order[2]", ValidationIssueCodes.UnknownId),
            ("key.order", ValidationIssueCodes.TooFew),
        ]);
    }

    [Fact]
    public void At_least_three_items_are_needed()
    {
        _module.ValidateAuthoring(Items(2), Key("all_or_nothing", "a", "b"), RationaleFor(2))
            .ShouldHaveSingleItem().Path.ShouldBe("content.items");
    }

    [Fact]
    public void An_unknown_scoring_rule_is_reported()
    {
        _module.ValidateAuthoring(Items(3), Key("half_marks", "a", "b", "c"), RationaleFor(3))
            .ShouldHaveSingleItem().Code.ShouldBe(ValidationIssueCodes.InvalidShape);
    }

    [Fact]
    public void A_response_must_place_every_item_once()
    {
        var issues = _module.ValidateResponse(Items(4), Json(new { order = new[] { "b", "a", "b" } }));

        issues.Select(i => (i.Path, i.Code)).ShouldBe(
            [("response.order[2]", ValidationIssueCodes.DuplicateId), ("response.order", ValidationIssueCodes.TooFew)]);
        _module.ValidateResponse(Items(4), Json(new { order = new[] { "d", "c", "b", "a" } })).ShouldBeEmpty();
    }

    [Fact]
    public void Max_points_is_1_for_all_or_nothing_and_the_item_count_per_position()
    {
        _module.MaxPoints(Items(4), Key("all_or_nothing", "a", "b", "c", "d")).ShouldBe(1);
        _module.MaxPoints(Items(4), Key("per_position", "a", "b", "c", "d")).ShouldBe(4);
    }

    [Fact]
    public void Present_shuffles_the_items_with_the_seed()
    {
        var content = Items(6);

        var first = ItemIds(_module.Present(content, "attempt-1:2"));

        first.ShouldBe(ItemIds(_module.Present(content, "attempt-1:2")));
        first.ShouldBe(["a", "b", "c", "d", "e", "f"], ignoreOrder: true);
        Enumerable.Range(0, 10).Select(i => string.Join(',', ItemIds(_module.Present(content, $"attempt-{i}:2"))))
            .Distinct().Count().ShouldBeGreaterThan(1);
    }

    private static JsonElement Items(int count) =>
        Json(new { items = Enumerable.Range(0, count).Select(i => new { id = OptionId(i), text = $"Step {OptionId(i)}" }) });

    private static JsonElement Key(string scoring, params string[] order) => Json(new { order, scoring });

    private static List<string> ItemIds(JsonElement content) =>
        [.. content.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetString()!)];
}
