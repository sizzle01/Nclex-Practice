using System.Net;

namespace Nclex.Domain.Audit;

/// <summary>
/// A record of a security event or admin action, kept for investigation. Maps to the
/// <c>audit_events</c> table (database.md section 2.9).
/// </summary>
/// <remarks>
/// Metadata must never hold passwords, tokens or answer keys, and should avoid personal data:
/// account deletion only clears <see cref="Ip"/> and the metadata of the user's own events.
/// </remarks>
public sealed class AuditEvent
{
    // For EF Core, which sets properties directly when loading rows.
    private AuditEvent()
    {
    }

    /// <summary>Database-generated (bigserial).</summary>
    public long Id { get; private set; }

    /// <summary>Who did it; null for anonymous actions such as a login with an unknown email.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>What happened, as a dotted name from <see cref="AuditActions"/>.</summary>
    public string Action { get; private set; } = string.Empty;

    /// <summary>The kind of thing acted on, e.g. "user".</summary>
    public string? EntityType { get; private set; }

    /// <summary>The id of the thing acted on, as text so any key type fits.</summary>
    public string? EntityId { get; private set; }

    /// <summary>Extra details as a JSON object; "{}" when there are none.</summary>
    public string Metadata { get; private set; } = "{}";

    /// <summary>The client's IP address, if known.</summary>
    public IPAddress? Ip { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Creates an event. <paramref name="metadataJson"/> must be a JSON object.</summary>
    public static AuditEvent Record(
        string action,
        Guid? actorId,
        string? entityType,
        string? entityId,
        string metadataJson,
        IPAddress? ip,
        DateTimeOffset now) =>
        new()
        {
            Action = action,
            ActorId = actorId,
            EntityType = entityType,
            EntityId = entityId,
            Metadata = metadataJson,
            Ip = ip,
            CreatedAt = now,
        };

    /// <summary>Removes the personal data an event may hold, when its user deletes their account.</summary>
    public void Anonymise()
    {
        Ip = null;
        Metadata = "{}";
    }
}
