using System.Text.Json;
using Nclex.Domain.ItemTypes;
using Nclex.Domain.ItemTypes.MultipleChoice;
using Nclex.Domain.ItemTypes.Sata;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>
/// Runs every golden case in ItemTypes/&lt;Type&gt;/fixtures/*.json through its module
/// (interfaces.md section 3, "Testing contract"). Add a case by adding JSON, not code.
/// </summary>
public sealed class GradingFixtureTests
{
    // The fixtures folder of each module, by the module's folder name.
    private static readonly Dictionary<string, IItemTypeModule> ModulesByFolder = new()
    {
        ["MultipleChoice"] = new MultipleChoiceModule(),
        ["Sata"] = new SataModule(),
    };

    private static string FixturesRoot => Path.Combine(AppContext.BaseDirectory, "ItemTypes");

    public static IEnumerable<TheoryDataRow<string, string>> Cases() =>
        from folder in ModulesByFolder.Keys
        from file in Directory.GetFiles(Path.Combine(FixturesRoot, folder, "fixtures"), "*.json")
        from fixture in ReadFixtures(file)
        select new TheoryDataRow<string, string>(Path.GetRelativePath(FixturesRoot, file), fixture.Name);

    [Theory]
    [MemberData(nameof(Cases))]
    public void Grades_the_golden_case(string fixtureFile, string caseName)
    {
        var fixture = ReadFixtures(Path.Combine(FixturesRoot, fixtureFile)).Single(f => f.Name == caseName);
        var module = ModulesByFolder[fixtureFile.Split(Path.DirectorySeparatorChar)[0]];

        var result = module.Grade(fixture.Content, fixture.Key, fixture.Response);

        result.Points.ShouldBe(fixture.ExpectedPoints);
        result.MaxPoints.ShouldBe(module.MaxPoints(fixture.Content, fixture.Key));
        result.IsCorrect.ShouldBe(fixture.ExpectedPoints == result.MaxPoints);
    }

    [Fact]
    public void Every_module_has_golden_cases()
    {
        foreach (var folder in ModulesByFolder.Keys)
        {
            Directory.GetFiles(Path.Combine(FixturesRoot, folder, "fixtures"), "*.json").ShouldNotBeEmpty(folder);
        }
    }

    private static List<GradingFixture> ReadFixtures(string file) =>
        JsonSerializer.Deserialize<List<GradingFixture>>(File.ReadAllText(file), JsonSerializerOptions.Web)!;

    private sealed record GradingFixture(string Name, JsonElement Content, JsonElement Key, JsonElement? Response, decimal ExpectedPoints);
}
