using System.Net;
using System.Net.Http.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.Expiry;
using Nclex.Application.Attempts.ListAttempts;
using Nclex.Application.Attempts.PauseAttempt;
using Nclex.Application.Attempts.SaveResponse;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Jobs;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Attempts;

/// <summary>
/// Server-authoritative timers, pause and expiry (architecture.md 5.1) through the real API, on a
/// host with a manual clock. Kept in one class so its tests run one at a time: the sweeper test
/// finalizes every overdue attempt in the shared database, which must only ever be its own.
/// </summary>
public sealed class AttemptTimerTests(ApiFactory factory) : IAsyncLifetime
{
    private const int BudgetSec = 600;

    private readonly CatalogContent _content = new(factory);
    private readonly TimedTestHost _host = new(factory);

    private AttemptsApi Attempts => _host.Attempts;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    // Exam timer

    [Fact]
    public async Task Opening_a_timed_attempt_starts_the_exam_timer_and_schedules_its_expiry_job()
    {
        var (user, attempt) = await StartedAsync(Timed());
        var openedAt = _host.Clock.GetUtcNow();

        var state = await Attempts.OpenOkAsync(user, attempt.Id, "c-1");

        attempt.Timer.Exam.ShouldBe(new ClockState(Running: false, DeadlineAt: null, RemainingMs: BudgetSec * 1000L));
        state.Timer.ServerNow.ShouldBe(openedAt);
        state.Timer.Exam.ShouldBe(new ClockState(true, openedAt.AddSeconds(BudgetSec), BudgetSec * 1000L));
        _host.Jobs.Expiries.ShouldBe([new(attempt.Id, 0, openedAt.AddSeconds(BudgetSec + 3))]);
    }

    [Fact]
    public async Task An_untimed_attempt_has_no_timers_and_schedules_nothing()
    {
        var (user, attempt) = await StartedAsync();

        var state = await Attempts.OpenOkAsync(user, attempt.Id, "c-1");

        (state.Timer.Exam, state.Timer.Item).ShouldBe((null, null));
        _host.Jobs.Expiries.ShouldBeEmpty();
    }

    [Fact]
    public async Task The_remaining_time_comes_from_the_server_clock_on_every_read()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        _host.Clock.Advance(TimeSpan.FromMinutes(4));
        var state = await Attempts.GetOkAsync(user, attempt.Id);
        _host.Clock.Advance(TimeSpan.FromMinutes(1));
        var drafts = await DraftsAsync(user);

        state.Timer.Exam!.ShouldSatisfyAllConditions(
            e => e.Running.ShouldBeTrue(),
            e => e.RemainingMs.ShouldBe((BudgetSec - 240) * 1000L));
        drafts.Items.Single(d => d.Id == attempt.Id).TimeRemainingMs.ShouldBe((BudgetSec - 300) * 1000L);
    }

    [Fact]
    public async Task Reopening_on_another_device_keeps_the_same_deadline()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        var deadline = (await Attempts.GetOkAsync(user, attempt.Id)).Timer.Exam!.DeadlineAt;

        _host.Clock.Advance(TimeSpan.FromMinutes(3));
        var phone = await Attempts.OpenOkAsync(user, attempt.Id, "phone");

        phone.Timer.Exam!.DeadlineAt.ShouldBe(deadline);
    }

    // Expiry rule 1: on every write

    [Fact]
    public async Task A_write_inside_the_two_second_grace_is_still_saved()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 2));
        var save = await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        save.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SaveResultAsync(save)).Timer.Exam!.RemainingMs.ShouldBe(0);
    }

    [Fact]
    public async Task A_write_after_the_grace_finalizes_the_attempt_and_is_refused_with_time_expired()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 3));
        var late = await Attempts.SaveAsync(user, attempt.Id, 2, "c-1", Answer("c"), 2);
        var again = await Attempts.NavigateAsync(user, attempt.Id, "c-1", 2);

        await late.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        await again.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        var row = await RowAsync(attempt.Id);
        row.ShouldSatisfyAllConditions(
            a => a.Attempt.Status.ShouldBe(AttemptStatus.Submitted),
            a => a.Attempt.SubmitReason.ShouldBe(SubmitReason.TimeExpired),
            a => a.Attempt.SubmittedAt.ShouldNotBeNull(),
            a => a.Items.ShouldAllBe(i => i.State == ItemState.Locked),
            a => a.Items[0].Response.ShouldNotBeNull(),
            a => a.Items[1].Response.ShouldBeNull());
    }

    [Fact]
    public async Task Opening_after_the_time_ran_out_with_the_browser_closed_finalizes_it()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        _host.Clock.Advance(TimeSpan.FromHours(3));
        var open = await Attempts.OpenAsync(user, attempt.Id, "c-2");

        await open.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        (await RowAsync(attempt.Id)).Attempt.Status.ShouldBe(AttemptStatus.Submitted);
    }

    [Fact]
    public async Task Discarding_after_the_time_ran_out_keeps_the_attempt_as_submitted()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 10));
        var discard = await Attempts.DiscardAsync(user, attempt.Id);

        await discard.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        (await RowAsync(attempt.Id)).Attempt.Status.ShouldBe(AttemptStatus.Submitted);
    }

    // Expiry rule 2: the delayed job

    [Fact]
    public async Task The_delayed_job_finalizes_the_attempt_when_it_runs_after_the_deadline()
    {
        var (_, attempt) = await OpenedAsync(Timed());
        var job = _host.Jobs.Expiries.Single();

        var early = await RunExpiryJobAsync(job);
        _host.Clock.Advance(job.At - _host.Clock.GetUtcNow());
        var onTime = await RunExpiryJobAsync(job);

        (early, onTime).ShouldBe((false, true));
        var row = await RowAsync(attempt.Id);
        (row.Attempt.Status, row.Attempt.SubmitReason).ShouldBe((AttemptStatus.Submitted, SubmitReason.TimeExpired));
        row.Items.ShouldAllBe(i => i.State == ItemState.Locked);
    }

    [Fact]
    public async Task The_job_class_runs_the_expiry_handler()
    {
        var (_, attempt) = await OpenedAsync(Timed());
        var scheduled = _host.Jobs.Expiries.Single();
        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 3));

        await _host.WithServiceAsync<ExpireAttemptHandler, bool>(async handler =>
        {
            await new AttemptExpiryJob(handler, NullLogger<AttemptExpiryJob>.Instance)
                .RunAsync(scheduled.AttemptId, scheduled.TimerEpoch, TestContext.Current.CancellationToken);
            return true;
        });

        (await RowAsync(attempt.Id)).Attempt.Status.ShouldBe(AttemptStatus.Submitted);
    }

    [Fact]
    public async Task A_job_scheduled_before_a_pause_does_nothing_and_the_new_one_takes_over()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauseSec: null));
        var beforePause = _host.Jobs.Expiries.Single();
        _host.Clock.Advance(TimeSpan.FromMinutes(2));
        await PauseOkAsync(user, attempt.Id, "break");
        _host.Clock.Advance(TimeSpan.FromMinutes(5));
        (await Attempts.ResumeAsync(user, attempt.Id, "c-1")).EnsureSuccessStatusCode();
        var afterResume = _host.Jobs.Expiries[^1];

        // The old deadline has passed, but the attempt still has 8 minutes after the pause.
        _host.Clock.Advance(beforePause.At - _host.Clock.GetUtcNow() + TimeSpan.FromSeconds(1));
        var stale = await RunExpiryJobAsync(beforePause);
        var stillRunning = await RowAsync(attempt.Id);
        _host.Clock.Advance(afterResume.At - _host.Clock.GetUtcNow());
        var current = await RunExpiryJobAsync(afterResume);

        afterResume.TimerEpoch.ShouldBe(beforePause.TimerEpoch + 2);
        stale.ShouldBeFalse();
        stillRunning.Attempt.Status.ShouldBe(AttemptStatus.InProgress);
        current.ShouldBeTrue();
        (await RowAsync(attempt.Id)).Attempt.Status.ShouldBe(AttemptStatus.Submitted);
    }

    // Expiry rule 3: the sweeper

    [Fact]
    public async Task The_sweeper_finalizes_running_attempts_over_30_seconds_past_their_deadline()
    {
        var (user, overdue) = await OpenedAsync(Timed(budgetSec: 60));
        var (_, recent) = await OpenedAsync(Timed(budgetSec: 75), user);
        var (_, paused) = await OpenedAsync(Timed(budgetSec: 60), user);
        await PauseOkAsync(user, paused.Id, "save_exit");

        _host.Clock.Advance(TimeSpan.FromSeconds(95)); // overdue: 35 s past; recent: 20 s past
        var finalized = await _host.WithServiceAsync<SweepExpiredAttemptsHandler, int>(
            sweeper => sweeper.HandleAsync(TestContext.Current.CancellationToken));

        finalized.ShouldBeGreaterThanOrEqualTo(1);
        (await RowAsync(overdue.Id)).Attempt.SubmitReason.ShouldBe(SubmitReason.TimeExpired);
        (await RowAsync(recent.Id)).Attempt.Status.ShouldBe(AttemptStatus.InProgress);
        (await RowAsync(paused.Id)).Attempt.Status.ShouldBe(AttemptStatus.Paused);
    }

    // Pause and resume

    [Fact]
    public async Task Pausing_freezes_the_timer_and_hides_the_content()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        _host.Clock.Advance(TimeSpan.FromMinutes(2));

        var paused = await PauseOkAsync(user, attempt.Id, "break");
        _host.Clock.Advance(TimeSpan.FromHours(1));
        var state = await Attempts.GetOkAsync(user, attempt.Id);
        var item = await Attempts.GetItemAsync(user, attempt.Id, 1);
        var save = await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("a"), 1);

        paused.Status.ShouldBe(AttemptStatus.Paused);
        paused.Timer.Exam.ShouldBe(new ClockState(false, null, (BudgetSec - 120) * 1000L));
        state.Status.ShouldBe(AttemptStatus.Paused);
        state.Timer.Exam!.RemainingMs.ShouldBe((BudgetSec - 120) * 1000L);
        state.Timer.Pause.ShouldSatisfyAllConditions(
            p => p.Used.ShouldBe(1),
            p => p.StartedAt.ShouldNotBeNull());
        await item.ShouldBeProblemAsync(HttpStatusCode.Locked, "ATTEMPT_PAUSED");
        await save.ShouldBeProblemAsync(HttpStatusCode.Locked, "ATTEMPT_PAUSED");
    }

    [Fact]
    public async Task Pausing_again_from_the_same_device_does_not_use_another_pause()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauses: 1));
        await PauseOkAsync(user, attempt.Id, "break");

        var retry = await PauseOkAsync(user, attempt.Id, "break");

        retry.Timer.Pause.Used.ShouldBe(1);
    }

    [Fact]
    public async Task Breaks_and_save_and_exit_share_the_pause_limit()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauses: 2));
        await PauseOkAsync(user, attempt.Id, "break");
        (await Attempts.ResumeAsync(user, attempt.Id, "c-1")).EnsureSuccessStatusCode();
        await PauseOkAsync(user, attempt.Id, "save_exit");
        (await Attempts.ResumeAsync(user, attempt.Id, "c-1")).EnsureSuccessStatusCode();

        var third = await Attempts.PauseAsync(user, attempt.Id, "c-1", new { reason = "break" });

        await third.ShouldBeProblemAsync(HttpStatusCode.Conflict, "PAUSE_LIMIT_REACHED");
        (await Attempts.GetOkAsync(user, attempt.Id)).Status.ShouldBe(AttemptStatus.InProgress);
    }

    [Fact]
    public async Task A_pause_needs_a_reason_and_the_lease()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        var missing = await Attempts.PauseAsync(user, attempt.Id, "c-1", new { });
        var unknown = await Attempts.PauseAsync(user, attempt.Id, "c-1", new { reason = "lunch" });
        var otherDevice = await Attempts.PauseAsync(user, attempt.Id, "phone", new { reason = "break" });

        (await missing.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("reason", "REQUIRED"));
        await unknown.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        await otherDevice.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
    }

    [Fact]
    public async Task Resuming_on_another_device_restarts_the_timer_there_and_schedules_a_new_job()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        _host.Clock.Advance(TimeSpan.FromMinutes(2));
        await PauseOkAsync(user, attempt.Id, "save_exit");
        _host.Clock.Advance(TimeSpan.FromSeconds(50)); // within the 60 s limit
        var resumedAt = _host.Clock.GetUtcNow();

        var response = await Attempts.ResumeAsync(user, attempt.Id, "phone");
        var laptop = await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("a"), 1);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var state = (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        state.Status.ShouldBe(AttemptStatus.InProgress);
        state.Timer.Exam.ShouldBe(new ClockState(true, resumedAt.AddSeconds(BudgetSec - 120), (BudgetSec - 120) * 1000L));
        _host.Jobs.Expiries[^1].ShouldBe(new(attempt.Id, 2, resumedAt.AddSeconds(BudgetSec - 120 + 3)));
        await laptop.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
    }

    [Fact]
    public async Task A_pause_past_its_limit_comes_off_the_exam_time()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauseSec: 60));
        await PauseOkAsync(user, attempt.Id, "break");
        _host.Clock.Advance(TimeSpan.FromMinutes(4)); // 3 min over

        var resumed = await Attempts.ResumeAsync(user, attempt.Id, "c-1");

        var state = (await resumed.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        state.Timer.Exam!.RemainingMs.ShouldBe((BudgetSec - 180) * 1000L);
    }

    [Fact]
    public async Task A_save_and_exit_that_overran_all_the_exam_time_finalizes_on_resume()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauseSec: 60));
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);
        await PauseOkAsync(user, attempt.Id, "save_exit");
        _host.Clock.Advance(TimeSpan.FromDays(1));

        var resumed = await Attempts.ResumeAsync(user, attempt.Id, "c-1");

        await resumed.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        var row = await RowAsync(attempt.Id);
        (row.Attempt.Status, row.Attempt.SubmitReason).ShouldBe((AttemptStatus.Submitted, SubmitReason.TimeExpired));
        row.Items[0].ShouldSatisfyAllConditions(i => i.State.ShouldBe(ItemState.Locked), i => i.Response.ShouldNotBeNull());
    }

    [Fact]
    public async Task A_paused_draft_never_expires()
    {
        var (user, attempt) = await OpenedAsync(Timed(maxPauses: null, maxPauseSec: null));
        await PauseOkAsync(user, attempt.Id, "save_exit");

        _host.Clock.Advance(TimeSpan.FromDays(40));
        await _host.WithServiceAsync<SweepExpiredAttemptsHandler, int>(s => s.HandleAsync(TestContext.Current.CancellationToken));
        var resumed = await Attempts.ResumeAsync(user, attempt.Id, "c-1");

        resumed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await resumed.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!
            .Timer.Exam!.RemainingMs.ShouldBe(BudgetSec * 1000L);
    }

    [Fact]
    public async Task Resuming_an_attempt_that_is_already_running_just_takes_the_lease()
    {
        var (user, attempt) = await OpenedAsync(Timed());

        var resumed = await Attempts.ResumeAsync(user, attempt.Id, "phone");
        var laptop = await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("a"), 1);

        resumed.StatusCode.ShouldBe(HttpStatusCode.OK);
        await laptop.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
    }

    // Item timers

    [Fact]
    public async Task An_item_whose_time_ran_out_is_locked_with_its_answer_and_the_next_one_starts()
    {
        var (user, attempt) = await OpenedAsync(new { timed = true, perQuestionSec = 60 });
        var opened = await Attempts.GetOkAsync(user, attempt.Id);
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(63));
        var late = await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("c"), 2);
        var item = await Attempts.GetItemOkAsync(user, attempt.Id, 1);
        var next = await Attempts.NavigateAsync(user, attempt.Id, "c-1", 2);

        opened.Timer.ShouldSatisfyAllConditions(
            t => t.Exam.ShouldBeNull(),
            t => t.Item!.Running.ShouldBeTrue(),
            t => t.Item!.RemainingMs.ShouldBe(60_000));
        await late.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ITEM_LOCKED");
        item.GetProperty("state").GetString().ShouldBe("locked");
        item.GetProperty("response").GetProperty("selected").GetString().ShouldBe("b");
        next.StatusCode.ShouldBe(HttpStatusCode.OK);
        var moved = await Attempts.GetOkAsync(user, attempt.Id);
        moved.Timer.Item!.ShouldSatisfyAllConditions(i => i.Running.ShouldBeTrue(), i => i.RemainingMs.ShouldBe(60_000));
    }

    [Fact]
    public async Task Leaving_an_item_freezes_its_timer_until_the_candidate_returns()
    {
        var (user, attempt) = await OpenedAsync(new { timed = true, perQuestionSec = 60 });
        _host.Clock.Advance(TimeSpan.FromSeconds(20));
        await Attempts.NavigateAsync(user, attempt.Id, "c-1", 2);

        _host.Clock.Advance(TimeSpan.FromSeconds(50));
        var back = await Attempts.NavigateAsync(user, attempt.Id, "c-1", 1);

        back.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await Attempts.GetOkAsync(user, attempt.Id)).Timer.Item!.RemainingMs.ShouldBe(40_000);
    }

    // Grading when time runs out (F10)

    [Fact]
    public async Task An_attempt_finalized_by_a_late_write_is_graded_with_its_blanks_scoring_zero()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 3));
        await Attempts.SaveAsync(user, attempt.Id, 2, "c-1", Answer("b"), 2);
        var result = await AttemptsApi.JsonOkAsync(await Attempts.ResultAsync(user, attempt.Id));

        result.GetProperty("submitReason").GetString().ShouldBe("time_expired");
        var score = result.GetProperty("score");
        (score.GetProperty("pointsEarned").GetDecimal(), score.GetProperty("maxPoints").GetDecimal(), score.GetProperty("itemsAnswered").GetInt32())
            .ShouldBe((1m, 3m, 1));
        var items = (await RowAsync(attempt.Id)).Items;
        items.Select(i => i.PointsEarned).ShouldBe([1m, 0m, 0m]);
        items.ShouldAllBe(i => i.GradedAt != null);
    }

    [Fact]
    public async Task Submitting_after_the_deadline_is_refused_and_a_retry_returns_the_time_expired_result()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 3));
        var late = await Attempts.SubmitAsync(user, attempt.Id, "c-1", new
        {
            finalResponses = new[] { new { position = 2, response = Answer("b"), clientSeq = 1 } },
        });
        var retry = await AttemptsApi.JsonOkAsync(await Attempts.SubmitAsync(user, attempt.Id, "c-1"));

        await late.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        retry.GetProperty("submitReason").GetString().ShouldBe("time_expired");
        // The answer sent after time was up does not count.
        retry.GetProperty("result").GetProperty("score").GetProperty("pointsEarned").GetDecimal().ShouldBe(1m);
    }

    [Fact]
    public async Task The_delayed_job_grades_the_attempt_it_finalizes()
    {
        var (user, attempt) = await OpenedAsync(Timed());
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("a"), 1);
        var job = _host.Jobs.Expiries.Single();

        _host.Clock.Advance(job.At - _host.Clock.GetUtcNow());
        await RunExpiryJobAsync(job);

        var result = await AttemptsApi.JsonOkAsync(await Attempts.ResultAsync(user, attempt.Id));
        (result.GetProperty("score").GetProperty("pointsEarned").GetDecimal(), result.GetProperty("score").GetProperty("maxPoints").GetDecimal())
            .ShouldBe((0m, 3m));
    }

    [Fact]
    public async Task The_sweeper_grades_the_attempts_it_finalizes()
    {
        var (user, attempt) = await OpenedAsync(Timed(budgetSec: 60));
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(95));
        await _host.WithServiceAsync<SweepExpiredAttemptsHandler, int>(
            sweeper => sweeper.HandleAsync(TestContext.Current.CancellationToken));

        var result = await AttemptsApi.JsonOkAsync(await Attempts.ResultAsync(user, attempt.Id));
        result.GetProperty("score").GetProperty("pointsEarned").GetDecimal().ShouldBe(1m);
    }

    [Fact]
    public async Task A_tutor_item_locked_by_its_timer_is_graded_and_revealed_at_once()
    {
        var (user, attempt) = await OpenedAsync(new { timed = true, perQuestionSec = 60, revealMode = "tutor" });
        await Attempts.SaveAsync(user, attempt.Id, 1, "c-1", Answer("b"), 1);

        _host.Clock.Advance(TimeSpan.FromSeconds(63));
        await Attempts.NavigateAsync(user, attempt.Id, "c-1", 2);
        var item = await Attempts.GetItemOkAsync(user, attempt.Id, 1);

        item.GetProperty("state").GetString().ShouldBe("locked");
        item.GetProperty("reveal").GetProperty("grade").GetProperty("isCorrect").GetBoolean().ShouldBeTrue();
        (await RowAsync(attempt.Id)).Items[0].GradedAt.ShouldNotBeNull();
    }

    // Real Hangfire (shared host)

    [Fact]
    public async Task The_api_schedules_the_expiry_job_in_hangfire()
    {
        var attempts = new AttemptsApi(factory, new AuthApi(factory));
        var (setId, _) = await ImportSetAsync(Timed());
        var user = (await attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await attempts.StartOkAsync(user, setId);

        await attempts.OpenOkAsync(user, attempt.Id, "c-1");

        var monitoring = factory.Services.GetRequiredService<JobStorage>().GetMonitoringApi();
        var scheduled = monitoring.ScheduledJobs(0, 1000).Select(j => j.Value.Job).ToList();
        scheduled.ShouldContain(job =>
            job.Type == typeof(AttemptExpiryJob)
            && job.Method.Name == nameof(AttemptExpiryJob.RunAsync)
            && (Guid)job.Args[0] == attempt.Id
            && (int)job.Args[1] == 0);
    }

    private static object Answer(string optionId) => new { selected = optionId };

    /// <summary>A challenge set with a 10-minute budget and the given pause limits.</summary>
    private static object Timed(int budgetSec = BudgetSec, int? maxPauses = 3, int? maxPauseSec = 60) => new
    {
        mode = "challenge",
        revealMode = "exam",
        timed = true,
        timeBudgetSec = budgetSec,
        pausePolicy = new { maxPauses, maxPauseSec },
    };

    private async Task<PauseAttemptResult> PauseOkAsync(string token, Guid attemptId, string reason)
    {
        var response = await Attempts.PauseAsync(token, attemptId, "c-1", new { reason });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<PauseAttemptResult>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private Task<bool> RunExpiryJobAsync(RecordingJobScheduler.ScheduledExpiry job) =>
        _host.WithServiceAsync<ExpireAttemptHandler, bool>(handler =>
            handler.HandleAsync(job.AttemptId, job.TimerEpoch, TestContext.Current.CancellationToken));

    private async Task<(string Token, AttemptState Attempt)> StartedAsync(object? settings = null, string? token = null)
    {
        var (setId, _) = await ImportSetAsync(settings);
        var user = token ?? (await Attempts.VerifiedUserAsync()).AccessToken;
        return (user, await Attempts.StartOkAsync(user, setId));
    }

    private async Task<(string Token, AttemptState Attempt)> OpenedAsync(object? settings = null, string? token = null)
    {
        var (user, attempt) = await StartedAsync(settings, token);
        await Attempts.OpenOkAsync(user, attempt.Id, "c-1");
        return (user, attempt);
    }

    private async Task<(Guid SetId, List<Guid> Questions)> ImportSetAsync(object? settings)
    {
        var (questions, setId) = (CatalogContent.NewIds(3), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, $"Timed {setId:N}", questions, settings)]));
        return (setId, questions);
    }

    private static async Task<SaveResponseResult> SaveResultAsync(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<SaveResponseResult>(AuthApi.Json, TestContext.Current.CancellationToken))!;

    private async Task<AttemptPage> DraftsAsync(string token)
    {
        var response = await Attempts.ListAsync(token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptPage>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    /// <summary>The attempt row and its items as stored.</summary>
    private async Task<(Attempt Attempt, List<AttemptItem> Items)> RowAsync(Guid attemptId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var attempt = await db.Attempts.AsNoTracking().SingleAsync(a => a.Id == attemptId, TestContext.Current.CancellationToken);
        var items = await db.AttemptItems.AsNoTracking()
            .Where(i => i.AttemptId == attemptId)
            .OrderBy(i => i.Position)
            .ToListAsync(TestContext.Current.CancellationToken);
        return (attempt, items);
    }
}
