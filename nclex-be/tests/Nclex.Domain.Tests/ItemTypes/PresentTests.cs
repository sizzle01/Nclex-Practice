using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.MultipleChoice;
using Nclex.Domain.ItemTypes.Sata;
using static Nclex.Domain.Tests.ItemTypes.ItemJsonTestData;

namespace Nclex.Domain.Tests.ItemTypes;

public sealed class PresentTests
{
    public static TheoryData<IItemTypeModule> Modules => [new MultipleChoiceModule(), new SataModule()];

    [Theory]
    [MemberData(nameof(Modules))]
    public void The_same_seed_always_gives_the_same_order(IItemTypeModule module)
    {
        var content = Json(Options(8));

        OptionIds(module.Present(content, "attempt-1:3")).ShouldBe(OptionIds(module.Present(content, "attempt-1:3")));
    }

    [Theory]
    [MemberData(nameof(Modules))]
    public void Keeps_every_option_and_its_text(IItemTypeModule module)
    {
        var content = Json(Options(8));

        var presented = module.Present(content, "attempt-1:3").Deserialize<MultipleChoiceContent>(ItemJson.Options)!;

        presented.Options.OrderBy(o => o.Id).ShouldBe(content.Deserialize<MultipleChoiceContent>(ItemJson.Options)!.Options);
    }

    [Fact]
    public void Different_seeds_give_different_orders()
    {
        var content = Json(Options(8));
        IItemTypeModule module = new SataModule();

        var orders = Enumerable.Range(0, 10).Select(i => string.Join(',', OptionIds(module.Present(content, $"attempt-{i}:1"))));

        orders.Distinct().Count().ShouldBeGreaterThan(1);
    }

    [Fact]
    public void Shuffling_does_not_change_the_grade()
    {
        IItemTypeModule module = new SataModule();
        var content = Json(Options(6));
        var key = SataKey("a", "d");
        var response = SataResponse("a", "b", "d");

        var shuffled = module.Present(content, "seed");

        module.Grade(shuffled, key, response).Points.ShouldBe(module.Grade(content, key, response).Points);
    }

    [Fact]
    public void SeededShuffle_order_is_fixed_for_a_known_seed()
    {
        // Pins the algorithm: if this changes, candidates would see options move when reopening an item.
        SeededShuffle.Shuffle(["a", "b", "c", "d"], "attempt:1").ShouldBe(["d", "a", "c", "b"]);
    }

    private static List<string> OptionIds(JsonElement content) =>
        [.. content.GetProperty("options").EnumerateArray().Select(o => o.GetProperty("id").GetString()!)];
}
