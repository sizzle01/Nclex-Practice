using System.Net;

namespace Nclex.Domain.Entities;

/// <summary>
/// One refresh token in a sign-in session. Maps to the <c>refresh_sessions</c> table
/// (database.md section 2.2).
/// </summary>
/// <remarks>
/// Rotation (architecture.md section 6): every refresh revokes the presented token and issues a
/// successor in the same <see cref="FamilyId"/>. A chain of rotations is one sign-in on one device.
/// If a token that was already rotated is presented again, someone may have stolen it, so the
/// whole family is revoked and both the thief and the real user must sign in again.
/// Only the SHA-256 hash of the token is stored, so a database leak does not leak usable tokens.
/// </remarks>
public sealed class RefreshSession
{
    /// <summary>How long a refresh token stays valid (api.md section 2: cookie Max-Age of 30 days).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    // For EF Core, which sets properties directly when loading rows.
    private RefreshSession()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Shared by every token in one rotation chain.</summary>
    public Guid FamilyId { get; private set; }

    /// <summary>SHA-256 of the token sent to the browser.</summary>
    public byte[] TokenHash { get; private set; } = [];

    public string? UserAgent { get; private set; }

    public IPAddress? Ip { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastUsedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    /// <summary>The successor issued when this token was rotated; null if revoked any other way.</summary>
    public Guid? ReplacedBy { get; private set; }

    public User? User { get; private set; }

    /// <summary>Starts a new family: called when a user signs up or logs in.</summary>
    public static RefreshSession Start(Guid userId, byte[] tokenHash, ClientInfo client, DateTimeOffset now) =>
        Create(userId, familyId: Guid.CreateVersion7(now), tokenHash, client, now);

    /// <summary>Works out whether this token can be used at <paramref name="now"/>.</summary>
    public RefreshSessionState StateAt(DateTimeOffset now)
    {
        if (RevokedAt is not null)
        {
            return RefreshSessionState.Revoked;
        }

        return now >= ExpiresAt ? RefreshSessionState.Expired : RefreshSessionState.Active;
    }

    /// <summary>
    /// Revokes this token and returns its successor in the same family. The caller must check
    /// <see cref="StateAt"/> first: only an active token can be rotated.
    /// </summary>
    public RefreshSession Rotate(byte[] newTokenHash, ClientInfo client, DateTimeOffset now)
    {
        if (StateAt(now) != RefreshSessionState.Active)
        {
            throw new InvalidOperationException("Only an active refresh session can be rotated.");
        }

        var successor = Create(UserId, FamilyId, newTokenHash, client, now);
        LastUsedAt = now;
        RevokedAt = now;
        ReplacedBy = successor.Id;
        return successor;
    }

    private static RefreshSession Create(Guid userId, Guid familyId, byte[] tokenHash, ClientInfo client, DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            FamilyId = familyId,
            TokenHash = tokenHash,
            UserAgent = client.UserAgent,
            Ip = client.Ip,
            CreatedAt = now,
            LastUsedAt = now,
            ExpiresAt = now + Lifetime,
        };
}

/// <summary>Whether a refresh token can be used.</summary>
public enum RefreshSessionState
{
    /// <summary>Not revoked and not expired: it may be rotated.</summary>
    Active,

    /// <summary>Past its expiry. The user must sign in again.</summary>
    Expired,

    /// <summary>Already rotated or logged out. Presenting it again counts as reuse.</summary>
    Revoked,
}

/// <summary>The device a session was created from, kept so users can recognise their sessions.</summary>
/// <param name="UserAgent">The browser's User-Agent header, if sent.</param>
/// <param name="Ip">The client's IP address, if known.</param>
public sealed record ClientInfo(string? UserAgent, IPAddress? Ip);
