using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Security;

/// <summary>
/// Opaque 256-bit random tokens (architecture.md section 6). Only the SHA-256 hash is stored.
/// A plain hash without salt is enough here: unlike passwords, the tokens are long and random, so
/// they cannot be guessed from the hash.
/// </summary>
public sealed class SecureTokenGenerator : ISecureTokenGenerator
{
    private const int TokenBytes = 32;

    public SecureToken Generate()
    {
        var value = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));
        return new SecureToken(value, Hash(value));
    }

    public byte[] Hash(string token) => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
