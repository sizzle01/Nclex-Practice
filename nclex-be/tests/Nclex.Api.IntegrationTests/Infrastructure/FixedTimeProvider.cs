namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>A clock stuck at one moment, for issuing tokens "in the past".</summary>
public sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
