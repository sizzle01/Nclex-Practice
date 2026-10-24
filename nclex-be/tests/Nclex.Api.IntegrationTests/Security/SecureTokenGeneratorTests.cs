using System.Security.Cryptography;
using System.Text;
using Nclex.Infrastructure.Security;

namespace Nclex.Api.IntegrationTests.Security;

public sealed class SecureTokenGeneratorTests
{
    private readonly SecureTokenGenerator _generator = new();

    [Fact]
    public void Generate_returns_a_url_safe_256_bit_token_and_its_sha256()
    {
        var token = _generator.Generate();

        token.Value.Length.ShouldBe(43); // 32 bytes in unpadded base64url
        token.Value.ShouldNotContain('+');
        token.Value.ShouldNotContain('/');
        token.Hash.ShouldBe(SHA256.HashData(Encoding.UTF8.GetBytes(token.Value)));
    }

    [Fact]
    public void Generate_never_repeats() =>
        _generator.Generate().Value.ShouldNotBe(_generator.Generate().Value);

    [Fact]
    public void Hash_matches_the_hash_from_generate()
    {
        var token = _generator.Generate();

        _generator.Hash(token.Value).ShouldBe(token.Hash);
    }
}
