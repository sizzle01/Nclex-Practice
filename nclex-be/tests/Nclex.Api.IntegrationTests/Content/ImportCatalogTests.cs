using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Content;

/// <summary>The banks and test sets part of the importer (F06).</summary>
public sealed class ImportCatalogTests(ApiFactory factory)
{
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task Sets_and_banks_are_created_once_and_then_left_unchanged()
    {
        var (questions, setId, bankId) = (CatalogContent.NewIds(3), Guid.NewGuid(), Guid.NewGuid());
        QuestionImportFile File() => CatalogContent.File("RN", questions,
            banks: [CatalogContent.Bank(bankId, "Bank", questions)],
            testSets: [CatalogContent.Set(setId, "Set", questions)]);

        var first = await _content.ImportAsync(File());
        var second = await _content.ImportAsync(File());

        (first.Banks, first.TestSets).ShouldBe((new ImportCounts(1, 0, 0), new ImportCounts(1, 0, 0)));
        (second.Banks, second.TestSets).ShouldBe((new ImportCounts(0, 0, 1), new ImportCounts(0, 0, 1)));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var set = await db.TestSets.SingleAsync(s => s.Id == setId, TestContext.Current.CancellationToken);
        (set.Kind, set.IsPublished, set.QuestionCount, set.Track).ShouldBe((TestSetKind.Prebuilt, true, 3, ExamTrack.RN));
        (await ItemsAsync(setId)).ShouldBe(questions);
        (await db.QuestionBanks.SingleAsync(b => b.Id == bankId, TestContext.Current.CancellationToken)).IsPublished.ShouldBeTrue();
        (await db.QuestionBankItems.CountAsync(i => i.BankId == bankId, TestContext.Current.CancellationToken)).ShouldBe(3);
    }

    [Fact]
    public async Task Changing_a_sets_order_and_settings_updates_it_in_place()
    {
        var (questions, setId) = (CatalogContent.NewIds(3), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets: [CatalogContent.Set(setId, "Set", questions)]));
        List<Guid> reordered = [questions[2], questions[0]];

        var result = await _content.ImportAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, "Renamed", reordered, new { mode = "challenge", timed = true, timeBudgetSec = 600 })]));

        result.TestSets.ShouldBe(new ImportCounts(0, 1, 0));
        (await ItemsAsync(setId)).ShouldBe(reordered);
        await using var scope = factory.Services.CreateAsyncScope();
        var set = await scope.ServiceProvider.GetRequiredService<AppDbContext>().TestSets
            .SingleAsync(s => s.Id == setId, TestContext.Current.CancellationToken);
        (set.Title, set.Mode, set.TimeBudgetSec, set.QuestionCount).ShouldBe(("Renamed", TestMode.Challenge, (int?)600, 2));
    }

    [Fact]
    public async Task A_set_can_use_questions_published_by_an_earlier_import()
    {
        var questions = CatalogContent.NewIds(2);
        await _content.ImportOkAsync(CatalogContent.File("RN", questions));
        var setId = Guid.NewGuid();

        // The new file brings one question of its own; the set uses the two published earlier.
        var result = await _content.ImportAsync(CatalogContent.File("RN", CatalogContent.NewIds(1), testSets:
            [CatalogContent.Set(setId, "Later", questions)]));

        result.Issues.ShouldBeEmpty();
        (await ItemsAsync(setId)).ShouldBe(questions);
    }

    [Fact]
    public async Task Problems_with_sets_and_banks_are_reported_and_nothing_is_written()
    {
        var questions = CatalogContent.NewIds(9);
        var pnQuestion = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.File("PN", [pnQuestion]));
        var unknown = Guid.NewGuid();
        var (setA, setB, bankId) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await _content.ImportAsync(CatalogContent.File("RN", questions,
            banks: [CatalogContent.Bank(bankId, "", [questions[0], questions[0]])],
            testSets:
            [
                CatalogContent.Set(setA, "Too small a sample", questions, new { isSample = true }),
                CatalogContent.Set(setB, "Broken", [unknown, pnQuestion], new { timed = true, passThreshold = 1.5m, mode = "cat" }),
                CatalogContent.Set(setB, "Again", questions),
            ]));

        result.Issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("banks[0].name", ImportIssueCodes.Required),
            ("banks[0].questionIds[1]", ImportIssueCodes.DuplicateId),
            ("testSets[0].questionIds", ImportIssueCodes.InvalidValue), // a sample set has 10 questions
            ("testSets[1].mode", ImportIssueCodes.InvalidValue),
            ("testSets[1].timed", ImportIssueCodes.InvalidValue),
            ("testSets[1].passThreshold", ImportIssueCodes.InvalidValue),
            ("testSets[1].questionIds[0]", ImportIssueCodes.UnknownQuestion),
            ("testSets[1].questionIds[1]", ImportIssueCodes.UnknownQuestion), // a PN question in an RN set
            ("testSets[2].id", ImportIssueCodes.DuplicateId),
        ]);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.TestSets.CountAsync(s => s.Id == setA || s.Id == setB, TestContext.Current.CancellationToken)).ShouldBe(0);
        (await db.Questions.CountAsync(q => questions.Contains(q.Id), TestContext.Current.CancellationToken)).ShouldBe(0);
    }

    [Fact]
    public async Task A_set_cannot_move_to_the_other_track()
    {
        var (questions, setId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets: [CatalogContent.Set(setId, "RN set", questions)]));
        var pnQuestions = CatalogContent.NewIds(1);

        var result = await _content.ImportAsync(CatalogContent.File("PN", pnQuestions, testSets: [CatalogContent.Set(setId, "PN set", pnQuestions)]));

        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("testSets[0].id"),
            i => i.Code.ShouldBe(ImportIssueCodes.TrackMismatch));
    }

    private async Task<List<Guid>> ItemsAsync(Guid setId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().TestSetItems
            .Where(i => i.TestSetId == setId)
            .OrderBy(i => i.Position)
            .Select(i => i.QuestionId)
            .ToListAsync(TestContext.Current.CancellationToken);
    }
}
