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
    public static Dictionary<string, object> Question(Guid id, string clientNeeds = "SECE.MOC") => new()
    {
        ["id"] = id,
        ["itemType"] = "multiple_choice",
        ["stem"] = "Which action should the nurse take first?",
        ["content"] = new { options = new[] { "a", "b", "c", "d" }.Select(o => new { id = o, text = $"Option {o}" }) },
        ["key"] = new { correct = "b" },
        ["rationale"] = new { correct = "Why b", perOption = new { a = "A", b = "B", c = "C", d = "D" } },
        ["educationalObjective"] = "Prioritise safety.",
        ["clientNeeds"] = clientNeeds,
        ["bodySystem"] = "fundamentals",
        ["difficulty"] = "easy",
    };

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

    public async Task<ImportResult> ImportAsync(QuestionImportFile file)
    {
        await ImportLock.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
            await using var scope = factory.Services.CreateAsyncScope();
            return await scope.ServiceProvider.GetRequiredService<ImportQuestionsHandler>()
                .HandleAsync(file, TestContext.Current.CancellationToken);
        }
        finally
        {
            ImportLock.Release();
        }
    }

    /// <summary>Imports a file and fails the test if the importer found problems.</summary>
    public async Task ImportOkAsync(QuestionImportFile file)
    {
        var result = await ImportAsync(file);
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

    /// <summary>Retires a question, as F14's admin tools will.</summary>
    public async Task RetireQuestionAsync(Guid questionId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Questions
            .Where(q => q.Id == questionId)
            .ExecuteUpdateAsync(q => q.SetProperty(x => x.Status, QuestionStatus.Retired), TestContext.Current.CancellationToken);
    }
}
