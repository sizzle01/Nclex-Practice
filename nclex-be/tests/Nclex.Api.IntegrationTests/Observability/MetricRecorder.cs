using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Common;

namespace Nclex.Api.IntegrationTests.Observability;

/// <summary>
/// Collects what the test API host records on the <see cref="NclexMetrics"/> meter while it is alive.
/// Tests share one host and run in parallel, so assertions look for a measurement, never an exact count.
/// </summary>
public sealed class MetricRecorder : IDisposable
{
    private readonly MeterListener _listener = new();

    public MetricRecorder(IServiceProvider hostServices)
    {
        // Each host's IMeterFactory owns its meters, so this only hears the host under test.
        var meterFactory = hostServices.GetRequiredService<IMeterFactory>();
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == NclexMetrics.MeterName && instrument.Meter.Scope == meterFactory)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _listener.SetMeasurementEventCallback<double>((instrument, value, tags, _) => Record(instrument, value, tags));
        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) => Record(instrument, value, tags));
        _listener.Start();
    }

    public ConcurrentQueue<Measurement> Measurements { get; } = new();

    /// <summary>Reads observable gauges (such as the outbox lag) now.</summary>
    public void CollectGauges() => _listener.RecordObservableInstruments();

    public bool Has(string instrument, string tag, string value) =>
        Measurements.Any(m => m.Instrument == instrument && Equals(m.Tags.GetValueOrDefault(tag), value));

    public void Dispose() => _listener.Dispose();

    private void Record(Instrument instrument, double value, ReadOnlySpan<KeyValuePair<string, object?>> tags) =>
        Measurements.Enqueue(new Measurement(instrument.Name, value, tags.ToArray().ToDictionary(t => t.Key, t => t.Value)));

    public sealed record Measurement(string Instrument, double Value, Dictionary<string, object?> Tags);
}
