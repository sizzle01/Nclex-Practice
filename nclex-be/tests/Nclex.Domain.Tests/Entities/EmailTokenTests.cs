using Nclex.Domain.Entities;

namespace Nclex.Domain.Tests.Entities;

public sealed class EmailTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public void Verification_token_lasts_24_hours()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.VerifyEmail, [1], Now);

        token.ExpiresAt.ShouldBe(Now.AddHours(24));
        token.IsUsableAt(token.ExpiresAt.AddTicks(-1)).ShouldBeTrue();
        token.IsUsableAt(token.ExpiresAt).ShouldBeFalse();
    }

    [Fact]
    public void Reset_token_lasts_30_minutes()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.ResetPassword, [1], Now);

        token.ExpiresAt.ShouldBe(Now.AddMinutes(30));
    }

    [Fact]
    public void Change_email_token_lasts_24_hours_and_carries_the_new_address()
    {
        var token = EmailToken.IssueForEmailChange(UserId, [1], "new@example.com", Now);

        token.Purpose.ShouldBe(EmailTokenPurpose.ChangeEmail);
        token.NewEmail.ShouldBe("new@example.com");
        token.ExpiresAt.ShouldBe(Now.AddHours(24));
        token.IsUsableAt(token.ExpiresAt.AddTicks(-1)).ShouldBeTrue();
        token.IsUsableAt(token.ExpiresAt).ShouldBeFalse();
    }

    [Fact]
    public void Change_email_tokens_cannot_be_issued_without_the_new_address()
    {
        Should.Throw<ArgumentException>(() => EmailToken.Issue(UserId, EmailTokenPurpose.ChangeEmail, [1], Now));
    }

    [Fact]
    public void Other_tokens_have_no_new_address()
    {
        EmailToken.Issue(UserId, EmailTokenPurpose.VerifyEmail, [1], Now).NewEmail.ShouldBeNull();
    }

    [Fact]
    public void Token_can_be_consumed_only_once()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.VerifyEmail, [1], Now);

        token.Consume(Now.AddMinutes(1));

        token.ConsumedAt.ShouldBe(Now.AddMinutes(1));
        token.IsUsableAt(Now.AddMinutes(2)).ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => token.Consume(Now.AddMinutes(2)));
    }

    [Fact]
    public void Expired_token_cannot_be_consumed()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.ResetPassword, [1], Now);

        Should.Throw<InvalidOperationException>(() => token.Consume(Now.AddMinutes(30)));
    }

    [Fact]
    public void Superseded_token_is_no_longer_usable()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.VerifyEmail, [1], Now);

        token.Supersede(Now.AddMinutes(5));

        token.IsUsableAt(Now.AddMinutes(6)).ShouldBeFalse();
    }

    [Fact]
    public void Superseding_keeps_the_original_consumed_time()
    {
        var token = EmailToken.Issue(UserId, EmailTokenPurpose.VerifyEmail, [1], Now);
        token.Consume(Now.AddMinutes(1));

        token.Supersede(Now.AddMinutes(5));

        token.ConsumedAt.ShouldBe(Now.AddMinutes(1));
    }
}
