using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth.EmailTokens;
using Nclex.Application.Common;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.ResendVerification;

/// <summary>
/// POST /auth/verify-email/resend: sends the signed-in user a new verification link, at most
/// <see cref="MaxPerHour"/> times in any hour (api.md section 2). Does nothing if already verified.
/// </summary>
public sealed class ResendVerificationHandler(
    IAppDbContext db,
    AccountEmails accountEmails,
    ICurrentUser currentUser,
    TimeProvider clock)
{
    /// <summary>Verification emails allowed per rolling hour, counting the one sent at sign-up.</summary>
    public const int MaxPerHour = 3;

    private static readonly TimeSpan Window = TimeSpan.FromHours(1);

    public async Task HandleAsync(CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken);
        if (user is not { IsDeleted: false })
        {
            throw new AppException(CommonErrors.Unauthenticated);
        }

        if (user.IsEmailVerified)
        {
            return;
        }

        await EnsureUnderLimitAsync(user.Id, cancellationToken);
        await accountEmails.QueueVerificationAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    // Every verification email has its own token, so counting the tokens created in the last hour
    // counts the emails. A simple per-user database count is enough until F03 adds the shared
    // Redis rate limiter; two requests at the very same moment could both slip under the limit.
    private async Task EnsureUnderLimitAsync(Guid userId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var windowStart = now - Window;
        var recent = await db.EmailTokens
            .Where(t => t.UserId == userId && t.Purpose == EmailTokenPurpose.VerifyEmail && t.CreatedAt > windowStart)
            .OrderBy(t => t.CreatedAt)
            .Select(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        if (recent.Count >= MaxPerHour)
        {
            // A slot frees up when the oldest email in the window turns an hour old.
            throw new AppException(CommonErrors.RateLimited, retryAfter: recent[0] + Window - now);
        }
    }
}
