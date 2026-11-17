namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Lets only one extra API host start at a time. Program.cs replaces Serilog's static bootstrap
/// logger and freezes it as the host builds, so two hosts starting at once (test classes run in
/// parallel) fail with "The logger is already frozen". Every per-test host (<see cref="TimedTestHost"/>,
/// <see cref="SecurityTestHost"/>, <see cref="RedisTestHost"/>) must start inside this lock;
/// guarding only some of them leaves the race open whenever test order changes.
/// </summary>
internal static class HostStartLock
{
    private static readonly Lock Gate = new();

    /// <summary>Runs <paramref name="start"/>, which builds and starts a host, while holding the lock.</summary>
    public static T Run<T>(Func<T> start)
    {
        lock (Gate)
        {
            return start();
        }
    }
}
