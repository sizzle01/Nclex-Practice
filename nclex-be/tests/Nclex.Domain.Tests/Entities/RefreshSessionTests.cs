using System.Net;
using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class RefreshSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);
    private static readonly ClientInfo Client = new("test-agent", IPAddress.Loopback);
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Start_creates_an_active_session_in_a_new_family_that_expires_in_30_days()
    {
        var session = RefreshSession.Start(UserId, [1, 2, 3], Client, Now);

        session.UserId.ShouldBe(UserId);
        session.FamilyId.ShouldNotBe(Guid.Empty);
        session.ExpiresAt.ShouldBe(Now.AddDays(30));
        session.StateAt(Now).ShouldBe(RefreshSessionState.Active);
    }

    [Fact]
    public void Each_start_creates_a_different_family()
    {
        var first = RefreshSession.Start(UserId, [1], Client, Now);
        var second = RefreshSession.Start(UserId, [2], Client, Now);

        first.FamilyId.ShouldNotBe(second.FamilyId);
    }

    [Fact]
    public void Session_is_expired_from_its_expiry_time()
    {
        var session = RefreshSession.Start(UserId, [1], Client, Now);

        session.StateAt(session.ExpiresAt.AddTicks(-1)).ShouldBe(RefreshSessionState.Active);
        session.StateAt(session.ExpiresAt).ShouldBe(RefreshSessionState.Expired);
    }

    [Fact]
    public void Rotate_revokes_the_old_token_and_links_it_to_a_successor_in_the_same_family()
    {
        var session = RefreshSession.Start(UserId, [1], Client, Now);
        var later = Now.AddHours(1);

        var successor = session.Rotate([2], Client, later);

        session.StateAt(later).ShouldBe(RefreshSessionState.Revoked);
        session.RevokedAt.ShouldBe(later);
        session.ReplacedBy.ShouldBe(successor.Id);
        successor.FamilyId.ShouldBe(session.FamilyId);
        successor.UserId.ShouldBe(UserId);
        successor.TokenHash.ShouldBe(new byte[] { 2 });
        successor.ExpiresAt.ShouldBe(later.AddDays(30));
        successor.StateAt(later).ShouldBe(RefreshSessionState.Active);
    }

    [Fact]
    public void A_rotated_token_reports_revoked_so_presenting_it_again_is_detected_as_reuse()
    {
        var session = RefreshSession.Start(UserId, [1], Client, Now);
        session.Rotate([2], Client, Now);

        session.StateAt(Now.AddMinutes(1)).ShouldBe(RefreshSessionState.Revoked);
        Should.Throw<InvalidOperationException>(() => session.Rotate([3], Client, Now.AddMinutes(1)));
    }

    [Fact]
    public void An_expired_token_cannot_be_rotated()
    {
        var session = RefreshSession.Start(UserId, [1], Client, Now);

        Should.Throw<InvalidOperationException>(() => session.Rotate([2], Client, session.ExpiresAt));
    }
}
