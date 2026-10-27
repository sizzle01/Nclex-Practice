namespace Nclex.Domain.Entities;

/// <summary>
/// A single-use token sent in an emailed link (verify email, reset password). Maps to the
/// <c>email_tokens</c> table (database.md section 2.2).
/// </summary>
/// <remarks>
/// Like refresh tokens, only the SHA-256 hash is stored (architecture.md section 6, "Email tokens"),
/// so a database leak does not leak usable links.
/// </remarks>
public sealed class EmailToken
{
    /// <summary>How long a verification link works (architecture.md section 6).</summary>
    public static readonly TimeSpan VerifyEmailLifetime = TimeSpan.FromHours(24);

    /// <summary>How long a password-reset link works (architecture.md section 6).</summary>
    public static readonly TimeSpan ResetPasswordLifetime = TimeSpan.FromMinutes(30);

    // For EF Core, which sets properties directly when loading rows.
    private EmailToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public EmailTokenPurpose Purpose { get; private set; }

    /// <summary>SHA-256 of the token in the emailed link.</summary>
    public byte[] TokenHash { get; private set; } = [];

    /// <summary>The address being confirmed. Only used for <see cref="EmailTokenPurpose.ChangeEmail"/>.</summary>
    public string? NewEmail { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    /// <summary>Set when the link is used, or when a newer link for the same purpose replaces it.</summary>
    public DateTimeOffset? ConsumedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public User? User { get; private set; }

    /// <summary>Creates a token for a verify-email or reset-password link.</summary>
    public static EmailToken Issue(Guid userId, EmailTokenPurpose purpose, byte[] tokenHash, DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            Purpose = purpose,
            TokenHash = tokenHash,
            ExpiresAt = now + LifetimeOf(purpose),
            CreatedAt = now,
        };

    /// <summary>True if the link has not been used or replaced, and has not expired.</summary>
    public bool IsUsableAt(DateTimeOffset now) => ConsumedAt is null && now < ExpiresAt;

    /// <summary>Marks the token as used. Tokens are single-use, so this only works once.</summary>
    public void Consume(DateTimeOffset now)
    {
        if (!IsUsableAt(now))
        {
            throw new InvalidOperationException("Only a usable email token can be consumed.");
        }

        ConsumedAt = now;
    }

    /// <summary>
    /// Retires a token because a newer link for the same purpose was sent, so only the latest link
    /// works. The DDL has a single consumed_at column, which is what makes a token unusable.
    /// </summary>
    public void Supersede(DateTimeOffset now) => ConsumedAt ??= now;

    private static TimeSpan LifetimeOf(EmailTokenPurpose purpose) => purpose switch
    {
        EmailTokenPurpose.VerifyEmail => VerifyEmailLifetime,
        EmailTokenPurpose.ResetPassword => ResetPasswordLifetime,
        _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, "Change-email tokens arrive in F03."),
    };
}
