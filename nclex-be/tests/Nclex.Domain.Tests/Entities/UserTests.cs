using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class UserTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 24, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Register_creates_a_free_unverified_candidate_in_utc()
    {
        var user = User.Register("ada@example.com", "hash", ExamTrack.PN, "Ada", Now);

        user.Role.ShouldBe(UserRole.Candidate);
        user.PlanTier.ShouldBe(PlanTier.Free);
        user.Track.ShouldBe(ExamTrack.PN);
        user.EmailVerifiedAt.ShouldBeNull();
        user.Timezone.ShouldBe("UTC");
        user.CreatedAt.ShouldBe(Now);
        user.UpdatedAt.ShouldBe(Now);
    }

    [Fact]
    public void UpdateProfile_changes_only_the_fields_given()
    {
        var user = User.Register("ada@example.com", "hash", ExamTrack.RN, "Ada", Now);
        var later = Now.AddDays(1);

        user.UpdateProfile(displayName: null, track: null, timezone: "Europe/London", targetExamDate: new DateOnly(2027, 3, 1), later);

        user.DisplayName.ShouldBe("Ada");
        user.Track.ShouldBe(ExamTrack.RN);
        user.Timezone.ShouldBe("Europe/London");
        user.TargetExamDate.ShouldBe(new DateOnly(2027, 3, 1));
        user.UpdatedAt.ShouldBe(later);
    }

    [Fact]
    public void UpdateProfile_with_empty_display_name_clears_it()
    {
        var user = User.Register("ada@example.com", "hash", ExamTrack.RN, "Ada", Now);

        user.UpdateProfile(displayName: "", track: null, timezone: null, targetExamDate: null, Now);

        user.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void MarkEmailVerified_records_the_first_verification_only()
    {
        var user = User.Register("ada@example.com", "hash", ExamTrack.RN, null, Now);

        user.MarkEmailVerified(Now.AddHours(1));
        user.MarkEmailVerified(Now.AddHours(2));

        user.IsEmailVerified.ShouldBeTrue();
        user.EmailVerifiedAt.ShouldBe(Now.AddHours(1));
        user.UpdatedAt.ShouldBe(Now.AddHours(1));
    }

    [Fact]
    public void ChangePassword_replaces_the_hash_and_records_when()
    {
        var user = User.Register("ada@example.com", "old-hash", ExamTrack.RN, null, Now);

        user.ChangePassword("new-hash", Now.AddDays(1));

        user.PasswordHash.ShouldBe("new-hash");
        user.PasswordChangedAt.ShouldBe(Now.AddDays(1));
        user.UpdatedAt.ShouldBe(Now.AddDays(1));
    }

    [Fact]
    public void A_new_account_is_not_locked()
    {
        var user = NewUser();

        user.IsLockedAt(Now).ShouldBeFalse();
        user.LockRemainingAt(Now).ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Lock_refuses_until_the_given_moment_and_then_ends_by_itself()
    {
        var user = NewUser();
        var until = Now.AddMinutes(15);

        user.Lock(until, Now);

        user.IsLockedAt(Now).ShouldBeTrue();
        user.LockRemainingAt(Now.AddMinutes(5)).ShouldBe(TimeSpan.FromMinutes(10));
        user.IsLockedAt(until.AddTicks(-1)).ShouldBeTrue();
        user.IsLockedAt(until).ShouldBeFalse();
        user.LockRemainingAt(until).ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public void Lock_keeps_a_longer_existing_lock()
    {
        var user = NewUser();
        user.Lock(Now.AddMinutes(30), Now);

        user.Lock(Now.AddMinutes(15), Now.AddMinutes(1));

        user.LockedUntil.ShouldBe(Now.AddMinutes(30));
    }

    [Fact]
    public void ChangeEmail_switches_the_address_and_marks_it_verified()
    {
        var user = NewUser();

        user.ChangeEmail("new@example.com", Now.AddHours(1));

        user.Email.ShouldBe("new@example.com");
        user.EmailVerifiedAt.ShouldBe(Now.AddHours(1));
        user.UpdatedAt.ShouldBe(Now.AddHours(1));
    }

    [Fact]
    public void ScrubForDeletion_removes_personal_data_and_keeps_the_row()
    {
        var user = NewUser();
        user.UpdateProfile(null, null, "Europe/London", new DateOnly(2027, 3, 1), Now);
        user.MarkEmailVerified(Now);
        user.Lock(Now.AddMinutes(15), Now);
        var later = Now.AddDays(3);

        user.ScrubForDeletion(later);

        user.Email.ShouldBe($"deleted+{user.Id}@invalid");
        user.PasswordHash.ShouldBeNull();
        user.DisplayName.ShouldBeNull();
        user.EmailVerifiedAt.ShouldBeNull();
        user.Timezone.ShouldBe("UTC");
        user.TargetExamDate.ShouldBeNull();
        user.LockedUntil.ShouldBeNull();
        user.DeletedAt.ShouldBe(later);
        user.IsDeleted.ShouldBeTrue();
        // Kept: the id (attempts point at it) and non-identifying facts used in statistics.
        user.Track.ShouldBe(ExamTrack.RN);
        user.CreatedAt.ShouldBe(Now);
    }

    [Fact]
    public void ScrubForDeletion_twice_keeps_the_first_deletion_time()
    {
        var user = NewUser();
        user.ScrubForDeletion(Now);

        user.ScrubForDeletion(Now.AddDays(1));

        user.DeletedAt.ShouldBe(Now);
    }

    private static User NewUser() => User.Register("ada@example.com", "hash", ExamTrack.RN, "Ada", Now);
}
