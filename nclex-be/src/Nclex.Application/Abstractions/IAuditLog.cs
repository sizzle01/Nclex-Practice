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
    /// Records an admin-console action (F14): who did <paramref name="action"/> to which entity.
    /// Never put answer keys or rationales in <paramref name="metadata"/>; ids and statuses are enough.
    /// </summary>
    /// <param name="action">A name from <see cref="Domain.Audit.AuditActions"/>, e.g. "question.publish".</param>
    /// <param name="actorId">The staff member who acted.</param>
    /// <param name="entityType">An <c>*Entity</c> constant from <see cref="Domain.Audit.AuditActions"/>.</param>
    /// <param name="entityId">The id of the thing acted on.</param>
    /// <param name="metadata">Extra details, stored as JSON.</param>
    void RecordAdminEvent(string action, Guid? actorId, string entityType, Guid entityId, object? metadata = null);
}
