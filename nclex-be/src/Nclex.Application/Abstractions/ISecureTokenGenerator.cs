namespace Nclex.Application.Abstractions;

/// <summary>Creates random opaque tokens (refresh tokens, and later email tokens) and hashes them for storage.</summary>
public interface ISecureTokenGenerator
{
    /// <summary>Creates a new 256-bit random token and its hash.</summary>
    SecureToken Generate();

    /// <summary>Hashes a token received from a client, to look up its stored hash.</summary>
    byte[] Hash(string token);
}

/// <summary>A token to send to the client, and the hash to store in the database.</summary>
/// <param name="Value">URL-safe text sent to the client. Never stored or logged.</param>
/// <param name="Hash">SHA-256 of <paramref name="Value"/>, stored in the database.</param>
public sealed record SecureToken(string Value, byte[] Hash);
