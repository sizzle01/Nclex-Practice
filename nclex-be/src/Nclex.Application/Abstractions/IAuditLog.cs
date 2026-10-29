using System.Net;

namespace Nclex.Application.Abstractions;

/// <summary>
/// Adds rows to <c>audit_events</c> (architecture.md section 6, "Audit"). Like the email outbox,
/// nothing is written here: the row is saved by the caller's next SaveChangesAsync, so an event is
/// recorded exactly when the change it describes is.
/// </summary>
public interface IAuditLog
{
    /// <summary>Records <paramref name="action"/> about the user <paramref name="userId"/>.</summary>
    /// <param name="action">A name from <see cref="Domain.Audit.AuditActions"/>.</param>
    /// <param name="userId">The account concerned, which is also the actor; null when unknown.</param>
    /// <param name="ip">The client's address, if known.</param>
    /// <param name="metadata">
    /// Extra details, stored as JSON. Never put passwords, tokens or email addresses here.
    /// </param>
    void RecordUserEvent(string action, Guid? userId, IPAddress? ip, object? metadata = null);

    /// <summary>
    /// Records <paramref name="action"/> by <paramref name="actorId"/> on one entity, e.g. a reviewer
    /// publishing a version (F14) or resolving a report (F15).
    /// </summary>
    /// <param name="action">A name from <see cref="Domain.Audit.AuditActions"/>, e.g. "question.publish".</param>
    /// <param name="actorId">The staff member who acted; null for a command-line change such as set-role.</param>
    /// <param name="entityType">An <c>*Entity</c> constant from <see cref="Domain.Audit.AuditActions"/>.</param>
    /// <param name="entityId">The id of the thing acted on.</param>
    /// <param name="metadata">Extra details, stored as JSON. Never put answer keys, rationales or personal data here.</param>
    void RecordEntityEvent(string action, Guid? actorId, string entityType, string entityId, object? metadata = null);
}
