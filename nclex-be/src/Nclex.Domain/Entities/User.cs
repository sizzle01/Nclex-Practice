namespace Nclex.Domain.Entities;

/// <summary>
/// A person with an account: a candidate, or staff (educator, author, reviewer, admin).
/// Maps to the <c>users</c> table (database.md section 2.2).
/// </summary>
public sealed class User
{
    /// <summary>Default time zone for new accounts, matching the column default.</summary>
    public const string DefaultTimezone = "UTC";

    /// <summary>Email of the account that imported content is attributed to; nothing is ever sent to it.</summary>
    public const string ContentImporterEmail = "content-import@system.local";

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

    /// <summary>Set after repeated failed logins or re-authentications; login is refused until then.</summary>
    public DateTimeOffset? LockedUntil { get; private set; }

    public DateTimeOffset? PasswordChangedAt { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Set when the account is deleted; the row is kept, anonymised (database.md retention notes).</summary>
    public DateTimeOffset? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt is not null;

    public bool IsEmailVerified => EmailVerifiedAt is not null;

    /// <summary>The address a deleted account's email is replaced with (database.md section 4).</summary>
    public static string DeletedEmailFor(Guid id) => $"deleted+{id}@invalid";

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
    /// Creates the system account that the content importer records as author of imported questions
    /// (question_versions.author_id is required). It has no password, so nobody can log in as it.
    /// </summary>
    public static User CreateContentImporter(DateTimeOffset now) =>
        new()
        {
            Id = Guid.CreateVersion7(now),
            Email = ContentImporterEmail,
            PasswordHash = null,
            Role = UserRole.Author,
            Track = ExamTrack.RN,
            PlanTier = PlanTier.Free,
            DisplayName = "Content importer",
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

    /// <summary>
    /// Records that the user proved they own their email address, by following a verification or
    /// password-reset link. The first verification time is kept.
    /// </summary>
    public void MarkEmailVerified(DateTimeOffset now)
    {
        if (EmailVerifiedAt is not null)
        {
            return;
        }

        EmailVerifiedAt = now;
        UpdatedAt = now;
    }

    /// <summary>Replaces the password hash, e.g. after a password reset.</summary>
    public void ChangePassword(string passwordHash, DateTimeOffset now)
    {
        PasswordHash = passwordHash;
        PasswordChangedAt = now;
        UpdatedAt = now;
    }

    /// <summary>True while a temporary lock is in force (architecture.md section 6, "Brute force").</summary>
    public bool IsLockedAt(DateTimeOffset now) => LockedUntil > now;

    /// <summary>How long until the lock ends; zero when the account is not locked.</summary>
    public TimeSpan LockRemainingAt(DateTimeOffset now) =>
        IsLockedAt(now) ? LockedUntil!.Value - now : TimeSpan.Zero;

    /// <summary>Refuses logins until <paramref name="until"/>. A longer existing lock is kept.</summary>
    public void Lock(DateTimeOffset until, DateTimeOffset now)
    {
        if (LockedUntil >= until)
        {
            return;
        }

        LockedUntil = until;
        UpdatedAt = now;
    }

    /// <summary>
    /// Switches the login address to <paramref name="newEmail"/> once the user has followed the
    /// link sent there, which also proves they own it.
    /// </summary>
    public void ChangeEmail(string newEmail, DateTimeOffset now)
    {
        Email = newEmail;
        EmailVerifiedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Deletes the account by removing its personal data and keeping the row, so attempts that
    /// point at it stay usable, anonymised, for item statistics (database.md section 4).
    /// The email becomes a placeholder, which frees the real address for a new sign-up.
    /// </summary>
    public void ScrubForDeletion(DateTimeOffset now)
    {
        if (IsDeleted)
        {
            return;
        }

        Email = DeletedEmailFor(Id);
        PasswordHash = null;
        EmailVerifiedAt = null;
        DisplayName = null;
        Timezone = DefaultTimezone;
        TargetExamDate = null;
        LockedUntil = null;
        DeletedAt = now;
        UpdatedAt = now;
    }
}
