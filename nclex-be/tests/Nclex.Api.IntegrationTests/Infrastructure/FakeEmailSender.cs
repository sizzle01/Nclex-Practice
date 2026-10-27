using System.Collections.Concurrent;
using Nclex.Application.Abstractions;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Records emails instead of sending them, and can be told to fail or to be slow.</summary>
public sealed class FakeEmailSender : IEmailSender
{
    private int _count;

    public ConcurrentQueue<EmailMessage> Sent { get; } = new();

    /// <summary>When set, every send throws this instead of succeeding.</summary>
    public Exception? FailWith { get; set; }

    /// <summary>How long each send takes, to widen race windows in concurrency tests.</summary>
    public TimeSpan Delay { get; set; }

    public async Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (Delay > TimeSpan.Zero)
        {
            await Task.Delay(Delay, cancellationToken);
        }

        if (FailWith is { } error)
        {
            throw error;
        }

        Sent.Enqueue(message);
        return $"fake-{Interlocked.Increment(ref _count)}";
    }

    public void Reset()
    {
        Sent.Clear();
        FailWith = null;
        Delay = TimeSpan.Zero;
    }
}
