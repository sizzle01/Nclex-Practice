using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Performance;

/// <summary>SnapshotRefreshJob and the rebuild (architecture.md 5.5) against the real database.</summary>
public sealed class SnapshotJobTests(ApiFactory factory) : IAsyncLifetime
{
    private readonly TimedTestHost _host = new(factory);
    private PerformanceScenario _scenario = null!;

    public async ValueTask InitializeAsync()
    {
        _scenario = new PerformanceScenario(factory, _host);
        await _scenario.ImportSetAsync();
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    [Fact]
    public async Task Submitting_queues_one_snapshot_refresh_for_the_attempt()
    {
        var candidate = await _scenario.CandidateAsync();

        var attemptId = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);

        _host.Jobs.SnapshotRefreshes.Where(j => j.UserId == candidate.Id)
            .ShouldHaveSingleItem().ShouldBe(new RecordingJobScheduler.SnapshotRefresh(candidate.Id, attemptId));
    }

    [Fact]
    public async Task A_rejected_write_queues_no_refresh()
    {
        var candidate = await _scenario.CandidateAsync();
        var attemptId = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);

        // Submitting again returns the stored result without finalizing anything.
        await AttemptsApi.JsonOkAsync(await _host.Attempts.SubmitAsync(candidate.Token, attemptId, "c-1"));

        _host.Jobs.SnapshotRefreshes.Count(j => j.UserId == candidate.Id).ShouldBe(1);
    }

    [Fact]
    public async Task The_job_adds_the_attempt_to_every_dimension()
    {
        var candidate = await _scenario.CandidateAsync();
        var attemptId = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);

        (await _scenario.RefreshAsync(candidate, attemptId)).ShouldBe(1);

        var rows = await _scenario.SnapshotsAsync(candidate);
        rows.ShouldContain($"Overall all 3/3 5.00/5.00 acc 1.0000 x1 last {attemptId}");
        rows.ShouldContain($"ClientNeeds SECE.SIC 1/1 3.00/3.00 acc 1.0000 x1 last {attemptId}");
        rows.ShouldContain($"ClientNeedsParent SECE 2/2 4.00/4.00 acc 1.0000 x1 last {attemptId}");
        rows.ShouldContain($"CjmmSkill take_action 1/1 3.00/3.00 acc 1.0000 x1 last {attemptId}");
    }

    [Fact]
    public async Task Running_the_job_twice_gives_the_same_totals()
    {
        var candidate = await _scenario.CandidateAsync();
        var attemptId = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        await _scenario.RefreshAsync(candidate, attemptId);
        var once = await _scenario.SnapshotsAsync(candidate);

        var addedAgain = await _scenario.RefreshAsync(candidate, attemptId);

        addedAgain.ShouldBe(0);
        (await _scenario.SnapshotsAsync(candidate)).ShouldBe(once);
    }

    [Fact]
    public async Task A_job_that_runs_late_finds_its_attempt_already_added()
    {
        var candidate = await _scenario.CandidateAsync();
        var first = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var second = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);

        // The second attempt's job runs first and catches up on both.
        (await _scenario.RefreshAsync(candidate, second)).ShouldBe(2);
        (await _scenario.RefreshAsync(candidate, first)).ShouldBe(0);

        var overall = await _scenario.OverallAsync(candidate);
        (overall.AttemptsCount, overall.ItemsAnswered, overall.PointsEarned, overall.MaxPoints, overall.Accuracy, overall.LastAttemptId)
            .ShouldBe((2, 6, 5.00m, 10.00m, 0.5000m, second));
    }

    [Fact]
    public async Task Jobs_running_at_the_same_time_add_each_attempt_once()
    {
        var candidate = await _scenario.CandidateAsync();
        var first = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var second = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);

        var added = await Task.WhenAll(
            _scenario.RefreshAsync(candidate, first),
            _scenario.RefreshAsync(candidate, second),
            _scenario.RefreshAsync(candidate, second));

        added.Sum().ShouldBe(2);
        (await _scenario.OverallAsync(candidate)).AttemptsCount.ShouldBe(2);
    }

    [Fact]
    public async Task Snapshots_that_disagree_with_the_attempts_are_replayed_from_scratch()
    {
        var candidate = await _scenario.CandidateAsync();
        var first = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        await _scenario.RefreshAsync(candidate, first);
        // As if an attempt committed late, behind the high-water mark: the count no longer matches.
        await CorruptOverallCountAsync(candidate.Id);
        var second = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);

        (await _scenario.RefreshAsync(candidate, second)).ShouldBe(2);

        var overall = await _scenario.OverallAsync(candidate);
        (overall.AttemptsCount, overall.ItemsAnswered, overall.Accuracy).ShouldBe((2, 6, 0.5000m));
    }

    [Fact]
    public async Task A_rebuild_gives_the_same_snapshots_as_the_jobs()
    {
        var candidate = await _scenario.CandidateAsync();
        foreach (var answers in new[] { PerformanceScenario.AllRight, PerformanceScenario.AllWrong, PerformanceScenario.AllRight })
        {
            var attemptId = await _scenario.SubmitAsync(candidate, answers);
            await _scenario.RefreshAsync(candidate, attemptId);
        }

        var incremental = await _scenario.SnapshotsAsync(candidate);

        (await _scenario.RebuildAsync(candidate)).ShouldBe(3);
        (await _scenario.SnapshotsAsync(candidate)).ShouldBe(incremental);
    }

    [Fact]
    public async Task A_rebuild_of_a_user_without_attempts_leaves_no_snapshots()
    {
        var candidate = await _scenario.CandidateAsync();

        (await _scenario.RebuildAsync(candidate)).ShouldBe(0);

        (await _scenario.SnapshotsAsync(candidate)).ShouldBeEmpty();
    }

    private async Task CorruptOverallCountAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().PerformanceSnapshots
            .Where(s => s.UserId == userId && s.DimensionKey == "all")
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AttemptsCount, 7), TestContext.Current.CancellationToken);
    }
}
