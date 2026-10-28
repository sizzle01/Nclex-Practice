using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.Lockout;
using Nclex.Application.Auth.Sessions;
using Nclex.Application.Common;
using Nclex.Application.Notifications;
using Nclex.Application.Users;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Domain.Security;

namespace Nclex.Application.Auth.Login;

/// <summary>
/// POST /auth/login: checks the email and password and starts a new session. Failed attempts
/// count toward a temporary lock (architecture.md section 6, "Brute force"); a locked account is
/// refused with 423 ACCOUNT_LOCKED even when the password is right.
/// </summary>
public sealed class LoginHandler(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    SessionService sessions,
    LoginThrottle throttle,
    AccountLockout lockout,
    SecurityNotifications securityNotifications,
    IAuditLog audit,
    TimeProvider clock)
{
    /// <summary>A browser counts as new if none of the user's sessions used it within this time.</summary>
    public static readonly TimeSpan KnownDeviceLookback = TimeSpan.FromDays(90);

    public async Task<SignInResult> HandleAsync(LoginRequest request, ClientInfo client, CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        var account = user is { IsDeleted: false } ? user : null;

        await ThrowIfLockedAsync(account, request.Email, client, cancellationToken);

        // Always verify, even when there is no account, so unknown emails take as long as wrong passwords.
        if (!passwordHasher.Verify(request.Password, account?.PasswordHash) || account is null)
        {
            await RecordFailureAsync(account, request.Email, client, cancellationToken);
            throw new AppException(AuthErrors.InvalidCredentials);
        }

        await throttle.ResetLoginFailuresAsync(request.Email, client.Ip, cancellationToken);
        if (await IsNewDeviceAsync(account, client, cancellationToken))
        {
            await securityNotifications.NewDeviceSignInAsync(account, client, cancellationToken);
        }

        var session = sessions.Start(account, client);
        audit.RecordUserEvent(AuditActions.LoginSucceeded, account.Id, client.Ip);
        await db.SaveChangesAsync(cancellationToken);
        return new SignInResult(UserProfile.From(account), session);
    }

    private async Task ThrowIfLockedAsync(User? account, string email, ClientInfo client, CancellationToken cancellationToken)
    {
        if (account is not null)
        {
            var remaining = account.LockRemainingAt(clock.GetUtcNow());
            if (remaining > TimeSpan.Zero)
            {
                audit.RecordUserEvent(AuditActions.LoginFailed, account.Id, client.Ip, new { reason = "locked" });
                await db.SaveChangesAsync(cancellationToken);
                throw AccountLockout.Locked(remaining);
            }

            return;
        }

        // No account: answer as a real account would after the same failures, so the 423 message
        // does not reveal which emails are registered.
        if (await throttle.UnknownEmailLockAsync(email, client.Ip, cancellationToken) is { } retryAfter)
        {
            throw AccountLockout.Locked(retryAfter);
        }
    }

    private async Task RecordFailureAsync(User? account, string email, ClientInfo client, CancellationToken cancellationToken)
    {
        var failures = await throttle.RecordLoginFailureAsync(email, client.Ip, cancellationToken);

        // Unknown emails are recorded without the address: audit rows must not collect personal
        // data about people who may not even have an account.
        audit.RecordUserEvent(
            AuditActions.LoginFailed,
            account?.Id,
            client.Ip,
            new { reason = account is null ? "unknown_email" : "wrong_password" });

        if (account is not null && LoginLockoutPolicy.ShouldLock(failures))
        {
            await lockout.LockAsync(account, client.Ip, reason: "failed_logins", cancellationToken);
            // The lock now does the blocking; a fresh count starts once it ends.
            await throttle.ResetLoginFailuresAsync(email, client.Ip, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    // A "device" is a browser, recognised by its User-Agent string. IP addresses change too often
    // (mobile networks, and the proxy issue in F01's follow-ups) to be useful on their own.
    private async Task<bool> IsNewDeviceAsync(User user, ClientInfo client, CancellationToken cancellationToken)
    {
        var since = clock.GetUtcNow() - KnownDeviceLookback;
        var seen = await db.RefreshSessions.AnyAsync(
            s => s.UserId == user.Id && s.UserAgent == client.UserAgent && s.CreatedAt > since,
            cancellationToken);
        return !seen;
    }
}
