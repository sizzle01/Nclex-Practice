using Nclex.Domain.Security;

namespace Nclex.Domain.Tests.Security;

public sealed class LoginLockoutPolicyTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(6, true)]
    public void The_fifth_failure_in_the_window_locks(long failures, bool locks)
    {
        LoginLockoutPolicy.ShouldLock(failures).ShouldBe(locks);
    }

    [Fact]
    public void A_lock_lasts_15_minutes()
    {
        var now = new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

        LoginLockoutPolicy.LockedUntil(now).ShouldBe(now.AddMinutes(15));
        LoginLockoutPolicy.Window.ShouldBe(TimeSpan.FromMinutes(15));
    }
}
