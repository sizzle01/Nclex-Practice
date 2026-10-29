using System.Diagnostics.Metrics;

namespace Nclex.Application.Common;

/// <summary>
/// The application's own metrics (architecture.md section 9). Every instrument lives on one meter,
/// <see cref="MeterName"/>, which the OpenTelemetry setup in each host exports; docs/operations.md
/// lists the alerts built on them. Registered as a singleton in both hosts.
/// </summary>
public sealed class NclexMetrics
{
    /// <summary>The meter name the hosts subscribe to.</summary>
    public const string MeterName = "Nclex";

    private readonly Histogram<double> _autosaveDuration;
    private readonly Counter<long> _errorResponses;
    private readonly Counter<long> _sweeperFinalizations;
    private readonly Histogram<double> _gradingDuration;
    private readonly UpDownCounter<long> _sseConnections;

    // Written by the outbox dispatcher each round and read whenever the exporter collects.
    private double _outboxLagSeconds;

    public NclexMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _autosaveDuration = meter.CreateHistogram<double>(
            "nclex.autosave.duration",
            unit: "s",
            description: "Time to handle one autosave, tagged with its outcome (saved, stale or the error code).");
        _errorResponses = meter.CreateCounter<long>(
            "nclex.api.error_responses",
            unit: "{response}",
            description: "Expected failures returned as ProblemDetails, tagged with their api.md error code.");
        _sweeperFinalizations = meter.CreateCounter<long>(
            "nclex.attempts.sweeper_finalizations",
            unit: "{attempt}",
            description: "Attempts the expiry sweeper had to finalize because their delayed job did not.");
        _gradingDuration = meter.CreateHistogram<double>(
            "nclex.grading.duration",
            unit: "s",
            description: "Time to grade an attempt and build its result, tagged with outcome (ok or failed).");
        _sseConnections = meter.CreateUpDownCounter<long>(
            "nclex.sse.connections",
            unit: "{connection}",
            description: "Notification streams currently open on this replica.");
        meter.CreateObservableGauge(
            "nclex.outbox.lag",
            () => Volatile.Read(ref _outboxLagSeconds),
            unit: "s",
            description: "How long the oldest due email has been waiting to be sent; 0 when the outbox is empty.");
    }

    /// <summary>Outcome tag for an autosave that changed the stored answer.</summary>
    public const string AutosaveSaved = "saved";

    /// <summary>Outcome tag for an autosave whose clientSeq was not newer (idempotent replay).</summary>
    public const string AutosaveStale = "stale";

    public void RecordAutosave(TimeSpan duration, string outcome) =>
        _autosaveDuration.Record(duration.TotalSeconds, new KeyValuePair<string, object?>("outcome", outcome));

    /// <summary>
    /// Counts one expected failure by code. ATTEMPT_TIME_EXPIRED rejections (architecture.md
    /// section 9) are this counter with code = ATTEMPT_TIME_EXPIRED.
    /// </summary>
    public void RecordErrorResponse(string code) =>
        _errorResponses.Add(1, new KeyValuePair<string, object?>("code", code));

    public void RecordSweeperFinalizations(int count) => _sweeperFinalizations.Add(count);

    public void RecordGrading(TimeSpan duration, bool succeeded) =>
        _gradingDuration.Record(duration.TotalSeconds, new KeyValuePair<string, object?>("outcome", succeeded ? "ok" : "failed"));

    public void SseConnectionOpened() => _sseConnections.Add(1);

    public void SseConnectionClosed() => _sseConnections.Add(-1);

    public void SetOutboxLag(TimeSpan lag) => Volatile.Write(ref _outboxLagSeconds, Math.Max(0, lag.TotalSeconds));
}
