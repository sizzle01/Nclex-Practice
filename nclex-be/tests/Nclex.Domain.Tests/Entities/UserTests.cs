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
}
