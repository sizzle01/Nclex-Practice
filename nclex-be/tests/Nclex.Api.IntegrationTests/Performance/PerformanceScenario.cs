using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Performance.Snapshots;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Performance;

/// <summary>
/// Users who finish attempts of one three-item set, on a host whose job scheduler records jobs
/// instead of running them, so each test decides when the snapshot jobs run. The set: a
/// multiple-choice item (SECE.MOC, key b), a SATA item (SECE.SIC, key a, b, e) and a
/// multiple-choice item tagged to PSI. Five points in all.
/// </summary>
internal sealed class PerformanceScenario(ApiFactory factory, TimedTestHost host)
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);

    /// <summary>Every answer right: 5 of 5 points.</summary>
    public static readonly Answers AllRight = new("b", ["a", "b", "e"], "b");

    /// <summary>Every answer wrong: 0 of 5 points.</summary>
    public static readonly Answers AllWrong = new("a", ["c", "d"], "a");

    public Guid SetId { get; private set; }

    public async Task ImportSetAsync()
    {
        var questions = CatalogContent.NewIds(3);
        SetId = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            [
                CatalogContent.Question(questions[0], "SECE.MOC", "fundamentals"),
                CatalogContent.SataQuestion(questions[1]),
                CatalogContent.Question(questions[2], "PSI", "mental-health"),
            ],
            testSets: [CatalogContent.Set(SetId, $"Performance {SetId:N}", questions, new { revealMode = "exam" })]));
    }

    public async Task<Candidate> CandidateAsync()
    {
        var user = await host.Attempts.VerifiedUserAsync();
        return new Candidate(user.Body.User.Id, user.Body.User.Email, user.AccessToken);
    }

    /// <summary>
    /// Starts, answers and submits an attempt of the set; returns its id. The host's clock moves on
    /// a minute first, so attempts submitted one after another keep that order in submitted_at.
    /// </summary>
    public async Task<Guid> SubmitAsync(Candidate candidate, Answers answers)
    {
        host.Clock.Advance(TimeSpan.FromMinutes(1));
        var attempt = await host.Attempts.StartOkAsync(candidate.Token, SetId);
        await host.Attempts.OpenOkAsync(candidate.Token, attempt.Id, Device);
        await host.Attempts.SaveAsync(candidate.Token, attempt.Id, 1, Device, new { selected = answers.First }, 1);
        await host.Attempts.SaveAsync(candidate.Token, attempt.Id, 2, Device, new { selected = answers.Sata }, 2);
        await host.Attempts.SaveAsync(candidate.Token, attempt.Id, 3, Device, new { selected = answers.Third }, 3);
        await AttemptsApi.JsonOkAsync(await host.Attempts.SubmitAsync(candidate.Token, attempt.Id, Device));
        return attempt.Id;
    }

    /// <summary>Runs SnapshotRefreshJob's handler for the attempt, as the worker would.</summary>
    public Task<int> RefreshAsync(Candidate candidate, Guid attemptId) =>
        host.WithServiceAsync<RefreshSnapshotsHandler, int>(h => h.HandleAsync(candidate.Id, attemptId, TestContext.Current.CancellationToken));

    public Task<int> RebuildAsync(Candidate candidate) =>
        host.WithServiceAsync<RebuildSnapshotsHandler, int>(h => h.HandleAsync(candidate.Id, TestContext.Current.CancellationToken));

    /// <summary>The candidate's snapshot rows as comparable text, e.g. "client_needs SECE.MOC 2/1 1.00/2.00 x2".</summary>
    public async Task<List<string>> SnapshotsAsync(Candidate candidate)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var rows = await scope.ServiceProvider.GetRequiredService<AppDbContext>().PerformanceSnapshots.AsNoTracking()
            .Where(s => s.UserId == candidate.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        return [.. rows
            .Select(s => $"{s.Dimension} {s.DimensionKey} {s.ItemsAnswered}/{s.ItemsCorrect} {s.PointsEarned:0.00}/{s.MaxPoints:0.00} "
                + $"acc {s.Accuracy:0.0000} x{s.AttemptsCount} last {s.LastAttemptId}")
            .Order(StringComparer.Ordinal)];
    }

    public Task<PerformanceSnapshot> OverallAsync(Candidate candidate) =>
        WithDbAsync(db => db.PerformanceSnapshots.AsNoTracking()
            .SingleAsync(s => s.UserId == candidate.Id && s.DimensionKey == "all", TestContext.Current.CancellationToken));

    /// <summary>Moves an attempt's submit time, for trend buckets (the API always stamps "now").</summary>
    public Task SetSubmittedAtAsync(Guid attemptId, string utc) =>
        WithDbAsync(db => db.Attempts.Where(a => a.Id == attemptId)
            .ExecuteUpdateAsync(a => a.SetProperty(x => x.SubmittedAt, DateTimeOffset.Parse(utc, System.Globalization.CultureInfo.InvariantCulture)), TestContext.Current.CancellationToken));

    public Task SetTimezoneAsync(Candidate candidate, string timezone) =>
        WithDbAsync(db => db.Users.Where(u => u.Id == candidate.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Timezone, timezone), TestContext.Current.CancellationToken));

    /// <summary>Makes the candidate an admin and signs them in again, so the new role is in their token.</summary>
    public async Task<string> AdminTokenAsync(Candidate candidate)
    {
        await WithDbAsync(db => db.Users.Where(u => u.Id == candidate.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.Role, UserRole.Admin), TestContext.Current.CancellationToken));
        return (await host.Auth.LoginOkAsync(candidate.Email)).AccessToken;
    }

    private async Task<T> WithDbAsync<T>(Func<AppDbContext, Task<T>> work)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await work(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public sealed record Candidate(Guid Id, string Email, string Token);

    public sealed record Answers(string First, string[] Sata, string Third);
}
