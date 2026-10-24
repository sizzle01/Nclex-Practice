namespace Nclex.Domain.Entities;

/// <summary>
/// A person with an account: a candidate, or staff (educator, author, reviewer, admin).
/// Maps to the <c>users</c> table (database.md section 2.2).
/// </summary>
public sealed class User
{
    /// <summary>Default time zone for new accounts, matching the column default.</summary>
    public const string DefaultTimezone = "UTC";

    // For EF Core, which sets properties directly when loading rows.
    private User()
    {
    }

    public Guid Id { get; private set; }

    /// <summary>Login email. Stored as <c>citext</c>, so uniqueness and lookups ignore case.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Encoded Argon2id hash. Null is reserved for future SSO-only accounts.</summary>
    public string? PasswordHash { get; private set; }

    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    public UserRole Role { get; private set; }

    public ExamTrack Track { get; private set; }

    public PlanTier PlanTier { get; private set; }

    public string? DisplayName { get; private set; }

    /// <summary>IANA time zone id (e.g. "Europe/London"), used for reminders and streaks.</summary>
    public string Timezone { get; private set; } = DefaultTimezone;

    public DateOnly? TargetExamDate { get; private set; }

    /// <summary>Set after repeated failed logins (rate limiting arrives in F03).</summary>
    public DateTimeOffset? LockedUntil { get; private set; }

    public DateTimeOffset? PasswordChangedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Set when the account is deleted; the row is kept, anonymised (database.md retention notes).</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    /// <summary>
    /// Creates a new candidate account. Every account starts as a free, unverified candidate
    /// (api.md section 2, POST /auth/signup).
    /// </summary>
    public static User Register(string email, string passwordHash, ExamTrack track, string? displayName, DateTimeOffset now) =>
        new()
        {
            // Version 7 GUIDs are time-ordered, which keeps the primary key index compact.
            Id = Guid.CreateVersion7(now),
            Email = email,
            PasswordHash = passwordHash,
            Role = UserRole.Candidate,
            Track = track,
            PlanTier = PlanTier.Free,
            DisplayName = displayName,
            Timezone = DefaultTimezone,
            CreatedAt = now,
            UpdatedAt = now,
        };

    /// <summary>
    /// Applies a profile edit (PATCH /me). Each argument that is null leaves that field unchanged.
    /// An empty display name clears it.
    /// </summary>
    public void UpdateProfile(
        string? displayName,
        ExamTrack? track,
        string? timezone,
        DateOnly? targetExamDate,
        DateTimeOffset now)
    {
        if (displayName is not null)
        {
            DisplayName = displayName.Length == 0 ? null : displayName;
        }

        Track = track ?? Track;
        Timezone = timezone ?? Timezone;
        TargetExamDate = targetExamDate ?? TargetExamDate;
        UpdatedAt = now;
    }
}
