using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Security;

/// <summary>
/// Argon2id password hashing with the parameters from architecture.md section 6
/// (64 MB of memory, 3 iterations).
/// </summary>
/// <remarks>
/// Hashes are stored in the standard PHC string format, which records the parameters next to the
/// salt and hash: <c>$argon2id$v=19$m=65536,t=3,p=1$&lt;salt&gt;$&lt;hash&gt;</c>. Verification reads the
/// parameters from the stored string, so they can be raised later without breaking old hashes.
/// </remarks>
public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    private const int MemoryKib = 64 * 1024;
    private const int Iterations = 3;
    private const int Parallelism = 1;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;
    private const string Prefix = "$argon2id$v=19$";

    // Hash of a random password, checked when an email has no account so that "no such user"
    // costs the same time as "wrong password". Built on first use (hashing takes ~100 ms).
    private readonly Lazy<string> _dummyHash = new(() => Encode(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))));

    public string Hash(string password) => Encode(password);

    private static string Encode(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Compute(password, salt, MemoryKib, Iterations, Parallelism, HashBytes);
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Prefix}m={MemoryKib},t={Iterations},p={Parallelism}${ToBase64(salt)}${ToBase64(hash)}");
    }

    public bool Verify(string password, string? passwordHash)
    {
        if (passwordHash is null)
        {
            Verify(password, _dummyHash.Value);
            return false;
        }

        // The Argon2 library rejects empty input; an empty password never matches anyway.
        if (password.Length == 0 || !TryParse(passwordHash, out var parameters, out var salt, out var expected))
        {
            return false;
        }

        var actual = Compute(password, salt, parameters.MemoryKib, parameters.Iterations, parameters.Parallelism, expected.Length);
        // Constant-time comparison: timing must not reveal how many bytes matched.
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKib, int iterations, int parallelism, int length)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKib,
            Iterations = iterations,
            DegreeOfParallelism = parallelism,
        };
        return argon2.GetBytes(length);
    }

    private static bool TryParse(string encoded, out Argon2Parameters parameters, out byte[] salt, out byte[] hash)
    {
        parameters = default;
        salt = hash = [];

        // "$argon2id$v=19$m=65536,t=3,p=1$salt$hash" splits into
        // ["", "argon2id", "v=19", "m=65536,t=3,p=1", salt, hash].
        var parts = encoded.Split('$');
        if (parts.Length != 6 || parts[1] != "argon2id" || parts[2] != "v=19")
        {
            return false;
        }

        var settings = new Dictionary<string, int>();
        foreach (var pair in parts[3].Split(','))
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length != 2 || !int.TryParse(keyValue[1], NumberStyles.None, CultureInfo.InvariantCulture, out var number))
            {
                return false;
            }

            settings[keyValue[0]] = number;
        }

        if (!settings.TryGetValue("m", out var memory) || !settings.TryGetValue("t", out var time) || !settings.TryGetValue("p", out var lanes))
        {
            return false;
        }

        parameters = new Argon2Parameters(memory, time, lanes);
        return TryFromBase64(parts[4], out salt) && TryFromBase64(parts[5], out hash) && hash.Length > 0;
    }

    // The PHC format uses standard Base64 without "=" padding.
    private static string ToBase64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=');

    private static bool TryFromBase64(string text, out byte[] bytes)
    {
        var padded = text.PadRight(text.Length + ((4 - (text.Length % 4)) % 4), '=');
        bytes = new byte[padded.Length * 3 / 4];
        if (!Convert.TryFromBase64String(padded, bytes, out var written))
        {
            bytes = [];
            return false;
        }

        bytes = bytes[..written];
        return true;
    }

    private readonly record struct Argon2Parameters(int MemoryKib, int Iterations, int Parallelism);
}
