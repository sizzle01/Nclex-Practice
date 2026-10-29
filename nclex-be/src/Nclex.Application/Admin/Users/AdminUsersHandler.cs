using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;

namespace Nclex.Application.Admin.Users;

/// <summary>
/// GET /admin/users and PATCH /admin/users/{id} (api.md section 8, admin only): role, plan tier
/// and unlock. Deleted accounts and the content importer's system account are not listed.
/// </summary>
public sealed class AdminUsersHandler(
    IAppDbContext db,
    SessionService sessions,
    ICurrentUser currentUser,
    IAuditLog audit,
    TimeProvider clock)
{
    // ToLower() here is translated to SQL lower(); it never runs in .NET, so the culture rules don't apply.
    // (Npgsql can't translate ToLowerInvariant.)
    [SuppressMessage("Globalization", "CA1304", Justification = "Translated to SQL lower() by EF Core.")]
    [SuppressMessage("Globalization", "CA1311", Justification = "Translated to SQL lower() by EF Core.")]
    public async Task<AdminUserPage> ListAsync(ListAdminUsersRequest request, CancellationToken cancellationToken)
    {
        var (limit, offset) = AdminPaging.Read(request.Limit, request.Cursor);
        var users = db.Users.AsNoTracking().Where(u => u.DeletedAt == null && u.Email != User.ContentImporterEmail);
        if (!string.IsNullOrWhiteSpace(request.Q))
        {
            var pattern = SearchPattern.Containing(request.Q);
            users = users.Where(u =>
                EF.Functions.Like(u.Email.ToLower(), pattern, SearchPattern.Escape)
                || (u.DisplayName != null && EF.Functions.Like(u.DisplayName.ToLower(), pattern, SearchPattern.Escape)));
        }

        var rows = await users
            .OrderBy(u => u.Email).ThenBy(u => u.Id)
            .Skip(offset)
            .Take(limit + 1)
            .Select(u => new AdminUserView(u.Id, u.Email, u.DisplayName, u.Role, u.PlanTier, u.Track, u.EmailVerifiedAt != null, u.LockedUntil, u.CreatedAt))
            .ToListAsync(cancellationToken);
        var (items, next) = AdminPaging.Page(rows, limit, offset);
        return new AdminUserPage(items, next);
    }

    public async Task<AdminUserView> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id && u.DeletedAt == null && u.Email != User.ContentImporterEmail, cancellationToken)
            ?? throw new AppException(AdminErrors.UserNotFound);
        var now = clock.GetUtcNow();
        var roleChanged = request.Role is { } role && role != user.Role;
        if (roleChanged && id == currentUser.UserId)
        {
            throw new AppException(AdminErrors.OwnRole);
        }

        var previousRole = user.Role;
        if (roleChanged)
        {
            user.ChangeRole(request.Role!.Value, now);
        }

        if (request.PlanTier is { } planTier && planTier != user.PlanTier)
        {
            user.ChangePlanTier(planTier, now);
        }

        if (request.Unlock == true)
        {
            user.Unlock(now);
        }

        audit.RecordEntityEvent(AuditActions.UserUpdated, currentUser.UserId, AuditActions.UserEntity, user.Id.ToString(), new
        {
            fromRole = roleChanged ? previousRole : (UserRole?)null,
            toRole = roleChanged ? user.Role : (UserRole?)null,
            request.PlanTier,
            unlocked = request.Unlock == true,
        });
        // The role is a claim in the 15-minute access token. Ending the user's refresh sessions means
        // they sign in again, with the new role, within 15 minutes at most instead of keeping the old
        // role for as long as they stay signed in. Same transaction, so both happen or neither does.
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        if (roleChanged)
        {
            await sessions.RevokeAllForUserAsync(user.Id, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        return new AdminUserView(user.Id, user.Email, user.DisplayName, user.Role, user.PlanTier, user.Track, user.IsEmailVerified, user.LockedUntil, user.CreatedAt);
    }
}
