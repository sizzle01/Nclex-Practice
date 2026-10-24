using Nclex.Infrastructure.Security;

namespace Nclex.Api.IntegrationTests.Security;

// Unit tests for Infrastructure security classes. They live here because this is the test project
// that references Nclex.Infrastructure (architecture.md section 4 lists no Infrastructure test project).
public sealed class Argon2idPasswordHasherTests
{
    private readonly Argon2idPasswordHasher _hasher = new();

    [Fact]
    public void Hash_uses_argon2id_with_the_architecture_parameters()
    {
        var hash = _hasher.Hash("correct-horse-battery");

        hash.ShouldStartWith("$argon2id$v=19$m=65536,t=3,p=1$");
        hash.Split('$').Length.ShouldBe(6);
    }

    [Fact]
    public void The_same_password_hashes_differently_each_time_because_of_the_salt() =>
        _hasher.Hash("correct-horse-battery").ShouldNotBe(_hasher.Hash("correct-horse-battery"));

    [Fact]
    public void Verify_accepts_the_right_password_and_rejects_others()
    {
        var hash = _hasher.Hash("correct-horse-battery");

        _hasher.Verify("correct-horse-battery", hash).ShouldBeTrue();
        _hasher.Verify("Correct-horse-battery", hash).ShouldBeFalse();
        _hasher.Verify("", hash).ShouldBeFalse();
    }

    [Fact]
    public void Verify_returns_false_when_there_is_no_hash() =>
        _hasher.Verify("correct-horse-battery", passwordHash: null).ShouldBeFalse();

    [Theory]
    [InlineData("")]
    [InlineData("plain-text")]
    [InlineData("$argon2i$v=19$m=65536,t=3,p=1$c2FsdA$aGFzaA")]
    [InlineData("$argon2id$v=19$m=lots,t=3,p=1$c2FsdA$aGFzaA")]
    [InlineData("$argon2id$v=19$m=65536,t=3,p=1$not base64!$aGFzaA")]
    public void Verify_returns_false_for_a_malformed_hash(string malformed) =>
        _hasher.Verify("correct-horse-battery", malformed).ShouldBeFalse();
}
