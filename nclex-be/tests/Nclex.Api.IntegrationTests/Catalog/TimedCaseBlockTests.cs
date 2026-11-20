using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Catalog;

/// <summary>
/// Timed case-study blocks (F24): the catalog writes them as timed exam-mode challenges of whole cases
/// (content/rn-bank/catalog.py). These tests import a block shaped the same way and take it as a candidate.
/// </summary>
public sealed class TimedCaseBlockTests(ApiFactory factory)
{
    private const string Device = "block-1";
    private const int SecondsPerQuestion = 120;    // the NCLEX-RN pace: 5 hours for up to 150 questions

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly Guid _firstCase = Guid.NewGuid();
    private readonly Guid _secondCase = Guid.NewGuid();
    private readonly List<Guid> _firstIds = CatalogContent.NewIds(3);
    private readonly List<Guid> _secondIds = CatalogContent.NewIds(3);
    private readonly Guid _blockId = Guid.NewGuid();

    [Fact]
    public async Task A_block_starts_with_exam_rules_and_the_nclex_time_budget()
    {
        await _content.ImportOkAsync(BlockFile());
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;

        var attempt = await _attempts.StartOkAsync(user, _blockId);

        attempt.TotalItems.ShouldBe(6);
        attempt.Settings.ShouldSatisfyAllConditions(
            s => s.Mode.ShouldBe(TestMode.Challenge),
            s => s.RevealMode.ShouldBe(RevealMode.Exam),
            s => s.AllowBackNav.ShouldBeFalse(),
            s => s.TimeBudgetSec.ShouldBe(6 * SecondsPerQuestion),
            // No pass estimate: a block is for finding where clinical judgment breaks down, not a verdict.
            s => s.PassThreshold.ShouldBeNull());
    }

    [Fact]
    public async Task A_block_keeps_each_case_whole_and_in_order()
    {
        await _content.ImportOkAsync(BlockFile());
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, _blockId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);

        var served = new List<(Guid Case, int Position)>();
        for (var position = 1; position <= 6; position++)
        {
            var caseStudy = (await _attempts.GetItemOkAsync(user, attempt.Id, position)).GetProperty("caseStudy");
            served.Add((caseStudy.GetProperty("id").GetGuid(), caseStudy.GetProperty("position").GetInt32()));
        }

        served.ShouldBe([(_firstCase, 1), (_firstCase, 2), (_firstCase, 3), (_secondCase, 1), (_secondCase, 2), (_secondCase, 3)]);
    }

    [Fact]
    public async Task A_block_allows_one_ten_minute_pause()
    {
        await _content.ImportOkAsync(BlockFile());

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var block = await db.TestSets.SingleAsync(s => s.Id == _blockId, TestContext.Current.CancellationToken);

        block.Settings.PausePolicy.ShouldBe(new PausePolicy(1, 600));
    }

    // Two three-question cases and a block of both, with the settings catalog.py writes for a timed block.
    private QuestionImportFile BlockFile()
    {
        var questions = CaseQuestions(_firstCase, _firstIds).Concat(CaseQuestions(_secondCase, _secondIds)).ToList();
        var block = CatalogContent.Set(_blockId, $"Timed case study block {_blockId:N}", [.. _firstIds, .. _secondIds], new
        {
            mode = "challenge",
            revealMode = "exam",
            timed = true,
            timeBudgetSec = 6 * SecondsPerQuestion,
            allowBackNav = false,
            pausePolicy = new { maxPauses = 1, maxPauseSec = 600 },
        });

        return QuestionImportFile.Parse(JsonSerializer.Serialize(new
        {
            track = "RN",
            questions,
            caseStudies = new[] { Case(_firstCase, "Heart failure"), Case(_secondCase, "Sepsis") },
            testSets = new[] { block },
        }));
    }

    private static IEnumerable<Dictionary<string, object>> CaseQuestions(Guid caseId, List<Guid> ids) =>
        ids.Select((id, i) =>
        {
            var question = CatalogContent.Question(id);
            question["caseStudyId"] = caseId;
            question["casePosition"] = i + 1;
            return question;
        });

    private static object Case(Guid id, string title) => new
    {
        id,
        title,
        scenario = new
        {
            tabs = new[] { new { id = "notes", title = "Nurses' notes", exhibits = new[] { new { id = "note", kind = "text", title = "0800", body = "Admitted overnight." } } } },
        },
    };
}
