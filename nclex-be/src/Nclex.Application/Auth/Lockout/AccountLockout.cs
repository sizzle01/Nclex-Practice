using System.Net;
using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Email;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.Security;

namespace Nclex.Application.Auth.Lockout;

/// <summary>
/// Locks an account after too many failed password checks, and refuses locked accounts with
/// 423 ACCOUNT_LOCKED. Does not save; the calling use case saves.
/// </summary>
public sealed class AccountLockout(SecurityEmails securityEmails, IAuditLog audit, TimeProvider clock)
{
    /// <summary>Throws 423 ACCOUNT_LOCKED, with the time left, if <paramref name="user"/> is locked.</summary>
    public void ThrowIfLocked(User user)
    {
        var remaining = user.LockRemainingAt(clock.GetUtcNow());
        if (remaining > TimeSpan.Zero)
        {
            throw Locked(remaining);
        }
    }

    /// <summary>Locks <paramref name="user"/> for the lock duration, records it and emails the owner.</summary>
    public void Lock(User user, IPAddress? ip, string reason)
    {
        var now = clock.GetUtcNow();
        var until = LoginLockoutPolicy.LockedUntil(now);
        user.Lock(until, now);
        audit.RecordUserEvent(AuditActions.AccountLocked, user.Id, ip, new { reason });
        securityEmails.QueueAccountLocked(user, until);
    }

    /// <summary>The 423 error, saying how long until the caller may try again.</summary>
    public static AppException Locked(TimeSpan retryAfter) => new(AuthErrors.AccountLocked, retryAfter);
}
