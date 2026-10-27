using System.Net;
using Nclex.Domain.Audit;

namespace Nclex.Domain.Tests.Audit;

public sealed class AuditEventTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Record_keeps_every_field()
    {
        var actor = Guid.NewGuid();

        var audit = AuditEvent.Record(AuditActions.PasswordChanged, actor, AuditActions.UserEntity, actor.ToString(), """{"a":1}""", IPAddress.Loopback, Now);

        audit.Action.ShouldBe("auth.password_changed");
        audit.ActorId.ShouldBe(actor);
        audit.EntityType.ShouldBe("user");
        audit.EntityId.ShouldBe(actor.ToString());
        audit.Metadata.ShouldBe("""{"a":1}""");
        audit.Ip.ShouldBe(IPAddress.Loopback);
        audit.CreatedAt.ShouldBe(Now);
    }
}
