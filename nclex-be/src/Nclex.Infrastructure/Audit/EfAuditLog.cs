using System.Net;
using System.Text.Json;
using Nclex.Application.Abstractions;
using Nclex.Domain.Audit;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Infrastructure.Audit;

/// <summary>Adds audit rows to the request's DbContext, saved with the use case's other changes.</summary>
internal sealed class EfAuditLog(AppDbContext db, TimeProvider clock) : IAuditLog
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public void RecordUserEvent(string action, Guid? userId, IPAddress? ip, object? metadata = null) =>
        db.AuditEvents.Add(AuditEvent.Record(
            action,
            actorId: userId,
            entityType: userId is null ? null : AuditActions.UserEntity,
            entityId: userId?.ToString(),
            metadata is null ? "{}" : JsonSerializer.Serialize(metadata, metadata.GetType(), Json),
            ip,
            clock.GetUtcNow()));
}
