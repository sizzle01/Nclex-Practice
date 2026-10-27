using Nclex.Application.Abstractions;
using Nclex.Application.Common;
using Nclex.Application.Common.Validation;
using Nclex.Domain.Entities;
using Nclex.Domain.Security;

namespace Nclex.Application.Auth.Lockout;

/// <summary>
/// Checks the current password before a sensitive account change (change password, change email,
/// delete account; api.md section 2). Wrong passwords count toward the same lockout as logins,
/// per user, so a stolen access token cannot be used to guess the password without limit.
/// </summary>
public sealed class Reauthentication(
    IAppDbContext db,
    IPasswordHasher passwordHasher,
    LoginThrottle throttle,
    AccountLockout lockout)
{
    /// <summary>The request field that holds the current password, named in error responses.</summary>
    public const string CurrentPasswordField = "currentPassword";

    /// <summary>
    /// Returns if <paramref name="currentPassword"/> is right. Otherwise throws: 423 ACCOUNT_LOCKED
    /// if the account is (or has just become) locked, else 400 VALIDATION_FAILED with
    /// <c>currentPassword: INCORRECT</c>. A 400, not 401, because 401 means "sign in again"
    /// to the frontend, which would refresh its token and send the wrong password a second time.
    /// </summary>
    public async Task VerifyAsync(User user, string currentPassword, ClientInfo client, CancellationToken cancellationToken)
    {
        lockout.ThrowIfLocked(user);

        if (passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            await throttle.ResetReauthFailuresAsync(user.Id, cancellationToken);
            return;
        }

        var failures = await throttle.RecordReauthFailureAsync(user.Id, cancellationToken);
        if (LoginLockoutPolicy.ShouldLock(failures))
        {
            lockout.Lock(user, client.Ip, reason: "reauthentication");
            // Saved now, because the request ends with an error instead of reaching its own save.
            await db.SaveChangesAsync(cancellationToken);
            await throttle.ResetReauthFailuresAsync(user.Id, cancellationToken);
            throw AccountLockout.Locked(LoginLockoutPolicy.LockDuration);
        }

        throw AppException.ForField(CurrentPasswordField, ValidationErrorCodes.Incorrect);
    }
}
