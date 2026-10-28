using System.Text.Json;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Reads Server-Sent Events from a streaming response, one event at a time.</summary>
public sealed class SseReader(Stream body) : IDisposable
{
    private readonly StreamReader _reader = new(body);

    /// <summary>The next event, or a failed test if none arrives in <paramref name="timeout"/>.</summary>
    public async Task<(string Event, JsonElement Data)> NextAsync(TimeSpan? timeout = null)
    {
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancel.CancelAfter(timeout ?? TimeSpan.FromSeconds(10));

        string? name = null;
        string? data = null;
        while (await _reader.ReadLineAsync(cancel.Token) is { } line)
        {
            if (line.Length == 0 && name is not null)
            {
                return (name, JsonDocument.Parse(data ?? "{}").RootElement.Clone());
            }

            if (line.StartsWith("event: ", StringComparison.Ordinal))
            {
                name = line["event: ".Length..];
            }
            else if (line.StartsWith("data: ", StringComparison.Ordinal))
            {
                data = line["data: ".Length..];
            }
        }

        throw new InvalidOperationException("The stream ended.");
    }

    /// <summary>The next event that is not a keep-alive ping.</summary>
    public async Task<(string Event, JsonElement Data)> NextNonPingAsync(TimeSpan? timeout = null)
    {
        while (true)
        {
            var next = await NextAsync(timeout);
            if (next.Event != "ping")
            {
                return next;
            }
        }
    }

    public void Dispose() => _reader.Dispose();
}
