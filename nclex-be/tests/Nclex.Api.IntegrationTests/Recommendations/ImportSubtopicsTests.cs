using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Recommendations;

/// <summary>The subtopic part of the importer: the automated half of tag review (docs/features/F17).</summary>
public sealed class ImportSubtopicsTests(ApiFactory factory)
{
    private static readonly string SampleFile = Path.Combine(AppContext.BaseDirectory, "content", "sample-questions.rn.json");

    private readonly CatalogContent _content = new(factory);

    private static string Unique => Guid.NewGuid().ToString("N");

    [Fact]
    public async Task Subtopics_and_tags_import_once_and_a_second_import_changes_nothing()
    {
        var (slug, question) = (Unique, Guid.NewGuid());
        QuestionImportFile Build() => File(track: "RN", [Declared("SECE.MOC", slug, $"Name {slug}")], [Tagged(question, "SECE.MOC", slug)]);

        var first = await _content.ImportAsync(Build());
        var second = await _content.ImportAsync(Build());

        first.Issues.ShouldBeEmpty();
        (first.Subtopics, second.Subtopics).ShouldBe((new ImportCounts(1, 0, 0), new ImportCounts(0, 0, 1)));
        second.Unchanged.ShouldBe(1);
        (await TagCountAsync(question)).ShouldBe(1);
    }

    [Fact]
    public async Task Reusing_a_slug_renames_the_subtopic()
    {
        var (slug, question) = (Unique, Guid.NewGuid());
        await _content.ImportOkAsync(File("RN", [Declared("SECE.MOC", slug, $"Old {slug}")], [Tagged(question, "SECE.MOC", slug)]));

        var result = await _content.ImportAsync(File("RN", [Declared("SECE.MOC", slug, $"New {slug}")], [Tagged(question, "SECE.MOC", slug)]));

        result.Subtopics.ShouldBe(new ImportCounts(0, 1, 0));
    }

    [Fact]
    public async Task Questions_without_a_subtopics_field_keep_their_tags()
    {
        var (slug, question) = (Unique, Guid.NewGuid());
        await _content.ImportOkAsync(File("RN", [Declared("SECE.MOC", slug, $"Name {slug}")], [Tagged(question, "SECE.MOC", slug)]));

        await _content.ImportOkAsync(File("RN", [], [CatalogContent.Question(question)]));

        (await TagCountAsync(question)).ShouldBe(1);
    }

    [Fact]
    public async Task A_listed_subtopic_no_question_uses_is_refused()
    {
        var (used, unused) = (Unique, Unique);

        var result = await _content.ImportAsync(File("RN", [Declared("SECE.MOC", used, $"A {used}"), Declared("SECE.MOC", unused, $"B {unused}")], [Tagged(Guid.NewGuid(), "SECE.MOC", used)]));

        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("subtopics[1].slug"),
            i => i.Code.ShouldBe(ImportIssueCodes.UnusedSubtopic));
    }

    [Fact]
    public async Task Two_names_for_one_topic_are_refused()
    {
        var (first, second, name) = (Unique, Unique, Unique);
        Dictionary<string, object>[] questions = [Tagged(Guid.NewGuid(), "SECE.MOC", first), Tagged(Guid.NewGuid(), "SECE.SIC", second)];

        var result = await _content.ImportAsync(File("RN", [Declared("SECE.MOC", first, $"Chest tube {name}"), Declared("SECE.SIC", second, $"chest tubes {name}!")], questions));

        result.Issues.ShouldContain(i => i.Code == ImportIssueCodes.DuplicateSubtopic);
    }

    [Fact]
    public async Task A_tag_from_another_category_is_refused()
    {
        var slug = Unique;

        var result = await _content.ImportAsync(File("RN", [Declared("SECE.MOC", slug, $"Name {slug}")], [Tagged(Guid.NewGuid(), "SECE.MOC", slug), Tagged(Guid.NewGuid(), "SECE.SIC", slug)]));

        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("questions[1].subtopics"),
            i => i.Code.ShouldBe(ImportIssueCodes.UnknownTag));
    }

    [Fact]
    public async Task An_rn_subtopic_can_not_tag_a_pn_question()
    {
        var (slug, rn) = (Unique, Guid.NewGuid());
        await _content.ImportOkAsync(File("RN", [Declared("SECE.MOC", slug, $"Name {slug}")], [Tagged(rn, "SECE.MOC", slug)]));

        var result = await _content.ImportAsync(File("PN", [], [Tagged(Guid.NewGuid(), "SECE.CC", slug)]));

        result.Issues.ShouldHaveSingleItem().Code.ShouldBe(ImportIssueCodes.UnknownTag);
    }

    [Fact]
    public async Task A_file_with_a_list_must_tag_every_question()
    {
        var slug = Unique;

        var result = await _content.ImportAsync(File("RN", [Declared("SECE.MOC", slug, $"Name {slug}")], [Tagged(Guid.NewGuid(), "SECE.MOC", slug), CatalogContent.Question(Guid.NewGuid())]));

        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("questions[1].subtopics"),
            i => i.Code.ShouldBe(ImportIssueCodes.Required));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task A_question_needs_one_to_three_tags(int count)
    {
        var slugs = Enumerable.Range(0, 4).Select(_ => Unique).ToList();
        var question = CatalogContent.Question(Guid.NewGuid());
        question["subtopics"] = slugs.Take(count).ToList();
        var extra = slugs.Skip(count).Select(s => Tagged(Guid.NewGuid(), "SECE.MOC", s));

        var result = await _content.ImportAsync(File("RN", [.. slugs.Select(s => Declared("SECE.MOC", s, $"Name {s}"))], [question, .. extra]));

        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("questions[0].subtopics"),
            i => i.Code.ShouldBe(ImportIssueCodes.InvalidValue));
    }

    [Fact]
    public async Task The_sample_file_tags_every_question_within_its_own_category()
    {
        var file = QuestionImportFile.Parse(await System.IO.File.ReadAllTextAsync(SampleFile, TestContext.Current.CancellationToken));

        var categoryOf = file.Subtopics!.ToDictionary(s => s.Slug, s => s.ClientNeeds);
        file.Questions.ShouldAllBe(q => q.Subtopics!.Count >= 1 && q.Subtopics.Count <= 3 && q.Subtopics.All(slug => categoryOf[slug] == q.ClientNeeds));
        categoryOf.Keys.ShouldBe(file.Questions.SelectMany(q => q.Subtopics!).Distinct(), ignoreOrder: true);
    }

    private static object Declared(string clientNeeds, string slug, string name) => new { clientNeeds, slug, name };

    private static Dictionary<string, object> Tagged(Guid id, string clientNeeds, string slug)
    {
        var question = CatalogContent.Question(id, clientNeeds);
        question["subtopics"] = new[] { slug };
        return question;
    }

    private static QuestionImportFile File(string track, object[] subtopics, IEnumerable<Dictionary<string, object>> questions) =>
        QuestionImportFile.Parse(JsonSerializer.Serialize(new { track, subtopics, questions }));

    private async Task<int> TagCountAsync(Guid questionId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().QuestionSubtopics
            .CountAsync(t => t.QuestionId == questionId, TestContext.Current.CancellationToken);
    }
}
