using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Infrastructure.Email;

namespace Nclex.Api.IntegrationTests.Observability;

/// <summary>The application metrics of architecture.md section 9, recorded by the real API.</summary>
public sealed class MetricsTests(ApiFactory factory)
{
    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    [Fact]
    public async Task Autosaves_record_their_latency_with_saved_stale_or_the_error_code()
    {
        using var metrics = new MetricRecorder(factory.Services);
        var (user, attemptId) = await OpenedAsync();

        (await _attempts.SaveAsync(user, attemptId, 1, "laptop", Answer("a"), 2)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _attempts.SaveAsync(user, attemptId, 1, "laptop", Answer("b"), 1)).StatusCode.ShouldBe(HttpStatusCode.OK);
        await _attempts.OpenOkAsync(user, attemptId, "phone");
        await (await _attempts.SaveAsync(user, attemptId, 1, "laptop", Answer("c"), 3))
            .ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");

        metrics.Has("nclex.autosave.duration", "outcome", "saved").ShouldBeTrue();
        metrics.Has("nclex.autosave.duration", "outcome", "stale").ShouldBeTrue();
        metrics.Has("nclex.autosave.duration", "outcome", "ATTEMPT_OPENED_ELSEWHERE").ShouldBeTrue();
        metrics.Has("nclex.api.error_responses", "code", "ATTEMPT_OPENED_ELSEWHERE").ShouldBeTrue();
    }

    [Fact]
    public async Task Submitting_records_how_long_grading_took()
    {
        using var metrics = new MetricRecorder(factory.Services);
        var (user, attemptId) = await OpenedAsync();

        (await _attempts.SubmitAsync(user, attemptId, "laptop")).StatusCode.ShouldBe(HttpStatusCode.OK);

        metrics.Has("nclex.grading.duration", "outcome", "ok").ShouldBeTrue();
    }

    [Fact]
    public async Task The_outbox_lag_gauge_reports_how_long_the_oldest_due_email_has_waited()
    {
        using var metrics = new MetricRecorder(factory.Services);
        await using var scope = factory.Services.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

        await dispatcher.MeasureLagAsync(TestContext.Current.CancellationToken);
        metrics.CollectGauges();

        // Other tests leave emails in the shared outbox, so the value itself is not fixed.
        metrics.Measurements.ShouldContain(m => m.Instrument == "nclex.outbox.lag" && m.Value >= 0);
    }

    private static object Answer(string optionId) => new { selected = optionId };

    private async Task<(string Token, Guid AttemptId)> OpenedAsync()
    {
        var (questions, setId) = (CatalogContent.NewIds(2), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, $"Metrics {setId:N}", questions)]));
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, "laptop");
        return (user, attempt.Id);
    }
}
