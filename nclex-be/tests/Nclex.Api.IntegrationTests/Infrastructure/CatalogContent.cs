using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Builds import files with fresh ids and loads them through the real importer, so catalog tests
/// get their own questions, banks and sets in the shared database without clashing.
/// </summary>
public sealed class CatalogContent(ApiFactory factory)
{
    // The importer creates its system user on first use with a check-then-insert, so two test
    // classes importing at the same moment on a fresh database can both try to insert it.
    // Imports run one at a time here; the worker's import command never runs concurrently.
    private static readonly SemaphoreSlim ImportLock = new(1, 1);

    /// <summary>A valid multiple-choice question; PN questions need a PN Client Needs code.</summary>
    public static Dictionary<string, object> Question(
        Guid id,
        string clientNeeds = "SECE.MOC",
        string bodySystem = "fundamentals",
        string difficulty = "easy",
        string accessTier = "free") => new()
    {
        ["id"] = id,
        ["itemType"] = "multiple_choice",
        ["stem"] = "Which action should the nurse take first?",
        ["content"] = new { options = new[] { "a", "b", "c", "d" }.Select(o => new { id = o, text = $"Option {o}" }) },
        ["key"] = new { correct = "b" },
        ["rationale"] = new { correct = "Why b", perOption = new { a = "A", b = "B", c = "C", d = "D" } },
        ["educationalObjective"] = "Prioritise safety.",
        ["clientNeeds"] = clientNeeds,
        ["bodySystem"] = bodySystem,
        ["difficulty"] = difficulty,
        ["accessTier"] = accessTier,
    };

    /// <summary>
    /// A Select All That Apply question with options a to e, of which a, b and e are correct, so
    /// a response can earn partial credit (+1 per right choice, -1 per wrong one, floor 0).
    /// </summary>
    public static Dictionary<string, object> SataQuestion(
        Guid id,
        string clientNeeds = "SECE.SIC",
        string bodySystem = "respiratory",
        string? cjmmSkill = "take_action")
    {
        var question = Question(id, clientNeeds, bodySystem, difficulty: "moderate");
        var options = new[] { "a", "b", "c", "d", "e" };
        question["itemType"] = "sata";
        question["stem"] = "Which findings should the nurse report? Select all that apply.";
        question["content"] = new { options = options.Select(o => new { id = o, text = $"Finding {o}" }) };
        question["key"] = new { correct = new[] { "a", "b", "e" } };
        question["rationale"] = new { correct = "Why a, b and e", perOption = options.ToDictionary(o => o, o => $"Because {o}") };
        if (cjmmSkill is not null)
        {
            question["cjmmSkill"] = cjmmSkill;
        }

        return question;
    }

    /// <summary>An untimed practice set; pass <paramref name="settings"/> to override any field.</summary>
    public static Dictionary<string, object?> Set(Guid id, string title, IEnumerable<Guid> questionIds, object? settings = null)
    {
        var set = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["title"] = title,
            ["mode"] = "practice",
            ["revealMode"] = "tutor",
            ["timed"] = false,
            ["questionIds"] = questionIds.ToList(),
        };
        foreach (var property in settings?.GetType().GetProperties() ?? [])
        {
            set[property.Name] = property.GetValue(settings);
        }

        return set;
    }

    public static Dictionary<string, object?> Bank(Guid id, string name, IEnumerable<Guid> questionIds, string accessTier = "free") => new()
    {
        ["id"] = id,
        ["name"] = name,
        ["accessTier"] = accessTier,
        ["questionIds"] = questionIds.ToList(),
    };

    public static List<Guid> NewIds(int count) => [.. Enumerable.Range(0, count).Select(_ => Guid.NewGuid())];

    /// <summary>Builds a file as JSON and parses it, so tests also cover the file format.</summary>
    public static QuestionImportFile File(
        string track,
        IEnumerable<Guid> questionIds,
        IEnumerable<object>? banks = null,
        IEnumerable<object>? testSets = null)
    {
        var clientNeeds = track == "PN" ? "SECE.CC" : "SECE.MOC";
        return QuestionImportFile.Parse(JsonSerializer.Serialize(new
        {
            track,
            questions = questionIds.Select(id => Question(id, clientNeeds)),
            banks = banks ?? [],
            testSets = testSets ?? [],
        }));
    }

    /// <summary>A file of questions built with <see cref="Question"/>, for tests that need varied tags.</summary>
    public static QuestionImportFile FileWith(
        string track,
        IEnumerable<Dictionary<string, object>> questions,
        IEnumerable<object>? testSets = null) =>
        QuestionImportFile.Parse(JsonSerializer.Serialize(new { track, questions, testSets = testSets ?? [] }));

    /// <summary>Imports a file; <paramref name="files"/> holds the media files beside it, if any.</summary>
    public async Task<ImportResult> ImportAsync(QuestionImportFile file, IContentFiles? files = null)
    {
        await ImportLock.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
            await using var scope = factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ImportQuestionsHandler>()
                .HandleAsync(file, files ?? NoContentFiles.Instance, TestContext.Current.CancellationToken);
        }
        finally
        {
            ImportLock.Release();
        }
    }

    /// <summary>Imports a file and fails the test if the importer found problems.</summary>
    public async Task ImportOkAsync(QuestionImportFile file, IContentFiles? files = null)
    {
        var result = await ImportAsync(file, files);
        result.Issues.ShouldBeEmpty();
    }

    /// <summary>Upgrades a user; there is no payment flow yet (docs/README.md assumption 3).</summary>
    public async Task MakePremiumAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.PlanTier, PlanTier.Premium), TestContext.Current.CancellationToken);
    }

    /// <summary>Takes a set out of the catalog; unpublishing has no use case until admin tools (F14).</summary>
    public async Task UnpublishSetAsync(Guid setId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().TestSets
            .Where(s => s.Id == setId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsPublished, false), TestContext.Current.CancellationToken);
    }

    /// <summary>Records that a user saw questions, as F10's grading will.</summary>
    public async Task AddHistoryAsync(Guid userId, bool? correct, params Guid[] questionIds)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.UserQuestionHistory.AddRange(questionIds.Select(q => UserQuestionHistory.FirstSeen(userId, q, correct, DateTimeOffset.UtcNow)));
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>The id of a Client Needs category by its code, e.g. "SECE" or "SECE.MOC".</summary>
    public async Task<short> CategoryIdAsync(ExamTrack track, string code)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().ClientNeedsCategories
            .Where(c => c.Track == track && c.Code == code)
            .Select(c => c.Id)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    public async Task<short> BodySystemIdAsync(string slug)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().BodySystems
            .Where(b => b.Slug == slug)
            .Select(b => b.Id)
            .SingleAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Puts published questions into a new case study, in the order given. The importer can't
    /// author case studies yet (F14), so this writes the rows directly.
    /// </summary>
    public async Task MakeCaseStudyAsync(params Guid[] questionIds)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caseId = Guid.NewGuid();
        var cancellationToken = TestContext.Current.CancellationToken;
        await db.Database.ExecuteSqlAsync(
            $"INSERT INTO case_studies (id, track, title, scenario) VALUES ({caseId}, 'RN', 'Test case', '{{}}'::jsonb)",
            cancellationToken);
        for (var i = 0; i < questionIds.Length; i++)
        {
            var position = (short)(i + 1);
            var questionId = questionIds[i];
            await db.Database.ExecuteSqlAsync(
                $"""
                UPDATE question_versions SET case_study_id = {caseId}, case_position = {position}
                WHERE id = (SELECT current_version_id FROM questions WHERE id = {questionId})
                """,
                cancellationToken);
        }
    }

    /// <summary>Retires a question, as F14's admin tools will.</summary>
    public async Task RetireQuestionAsync(Guid questionId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Questions
            .Where(q => q.Id == questionId)
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.Status, QuestionStatus.Retired), TestContext.Current.CancellationToken);
    }
}
