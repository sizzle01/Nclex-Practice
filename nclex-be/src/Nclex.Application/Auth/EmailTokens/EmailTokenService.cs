using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Auth.EmailTokens;

/// <summary>
/// Issues and looks up the single-use tokens in emailed links (architecture.md section 6, "Email tokens").
/// Methods that add or change rows do not save; the calling use case saves once, in one transaction.
/// </summary>
public sealed class EmailTokenService(IAppDbContext db, ISecureTokenGenerator tokens, TimeProvider clock)
{
    /// <summary>
    /// Creates a new token for <paramref name="purpose"/> and returns its raw value for the link.
    /// Earlier unused tokens for the same purpose stop working, so only the latest email's link works.
    /// </summary>
    public async Task<string> IssueAsync(User user, EmailTokenPurpose purpose, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();

        var olderTokens = await db.EmailTokens
            .Where(t => t.UserId == user.Id && t.Purpose == purpose && t.ConsumedAt == null)
            .ToListAsync(cancellationToken);
        foreach (var older in olderTokens)
        {
            older.Supersede(now);
        }

        var token = tokens.Generate();
        db.EmailTokens.Add(EmailToken.Issue(user.Id, purpose, token.Hash, now));
        return token.Value;
    }

    /// <summary>
    /// Finds the token from a link, with its user, if it can still be used at <paramref name="now"/>.
    /// Returns null for unknown, expired, used or superseded tokens, tokens for another purpose, and
    /// deleted accounts; callers report all of these the same way, so a link's state is not revealed.
    /// </summary>
    public async Task<EmailToken?> FindUsableAsync(
        string token,
        EmailTokenPurpose purpose,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var tokenHash = tokens.Hash(token);
        var emailToken = await db.EmailTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash && t.Purpose == purpose, cancellationToken);

        return emailToken is { User.IsDeleted: false } && emailToken.IsUsableAt(now) ? emailToken : null;
    }
}
