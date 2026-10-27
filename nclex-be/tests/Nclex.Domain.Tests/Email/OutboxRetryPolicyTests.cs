using Nclex.Domain.Email;

namespace Nclex.Domain.Tests.Email;

public sealed class OutboxRetryPolicyTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1, 30)]
    [InlineData(2, 120)]
    [InlineData(3, 600)]
    [InlineData(4, 3600)]
    public void Waits_longer_after_each_failure(int attemptsMade, int expectedDelaySeconds)
    {
        OutboxRetryPolicy.NextAttemptAt(attemptsMade, Now).ShouldBe(Now.AddSeconds(expectedDelaySeconds));
    }

    [Fact]
    public void Gives_up_after_the_last_attempt()
    {
        OutboxRetryPolicy.NextAttemptAt(OutboxRetryPolicy.MaxAttempts, Now).ShouldBeNull();
    }

    [Fact]
    public void Rejects_zero_attempts()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => OutboxRetryPolicy.NextAttemptAt(0, Now));
    }
}
