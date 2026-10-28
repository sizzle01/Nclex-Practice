using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Application.Notifications;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.Flags;

namespace Nclex.Application.Flags.Moderation;

/// <summary>
/// PATCH /admin/flags/{id}: assign, triage, resolve or reject a report (api.md section 8).
/// Resolving notifies the reporter in the same transaction; rejecting does not. Closing is audited.
/// </summary>
public sealed class UpdateFlagHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    INotificationPublisher notifications,
    IAuditLog audit,
    FlagQueueReader reader,
    TimeProvider clock)
{
    /// <summary>Roles that moderate reports, and so may be assigned one (architecture.md section 6).</summary>
    public static readonly UserRole[] ModeratorRoles = [UserRole.Reviewer, UserRole.Admin];

    public async Task<FlagQueueItem> HandleAsync(Guid flagId, UpdateFlagRequest request, CancellationToken cancellationToken)
    {
        var flag = await db.QuestionFlags.SingleOrDefaultAsync(f => f.Id == flagId, cancellationToken)
            ?? throw new AppException(FlagErrors.FlagNotFound);

        // Closed is final, so the reporter is never notified twice.
        if (flag.IsClosed)
        {
            throw new AppException(FlagErrors.Closed);
        }

        if (request.AssigneeId is { } assigneeId)
        {
            await EnsureModeratorAsync(assigneeId, cancellationToken);
            flag.AssignTo(assigneeId);
        }

        await ApplyStatusAsync(flag, request, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return (await reader.ReadAsync([flag], cancellationToken))[0];
    }

    private async Task ApplyStatusAsync(QuestionFlag flag, UpdateFlagRequest request, CancellationToken cancellationToken)
    {
        var moderatorId = currentUser.UserId;
        switch (request.Status)
        {
            case FlagStatus.Triaged:
                flag.Triage();
                break;
            case FlagStatus.Resolved:
                flag.Resolve(request.ResolutionNote!, moderatorId, clock.GetUtcNow());
                var reporterName = await db.Users.Where(u => u.Id == flag.UserId).Select(u => u.DisplayName).SingleAsync(cancellationToken);
                await notifications.PublishAsync(FlagNotifications.Resolved(flag, reporterName), cancellationToken);
                RecordClosed(AuditActions.FlagResolved, flag);
                break;
            case FlagStatus.Rejected:
                flag.Reject(request.ResolutionNote!, moderatorId, clock.GetUtcNow());
                RecordClosed(AuditActions.FlagRejected, flag);
                break;
        }
    }

    private void RecordClosed(string action, QuestionFlag flag) =>
        // Ids and the reason only: the note may quote the question, and the comment is the reporter's words.
        audit.RecordEntityEvent(
            action,
            currentUser.UserId,
            AuditActions.FlagEntity,
            flag.Id.ToString(),
            new { questionVersionId = flag.QuestionVersionId, reason = JsonNamingPolicy.SnakeCaseLower.ConvertName(flag.Reason.ToString()) });

    private async Task EnsureModeratorAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!await db.Users.AnyAsync(u => u.Id == userId && ModeratorRoles.Contains(u.Role) && u.DeletedAt == null, cancellationToken))
        {
            throw AppException.ForField("assigneeId", ValidationErrorCodes.InvalidValue);
        }
    }
}
