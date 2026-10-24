namespace Nclex.Application.Abstractions;

/// <summary>Hashes and checks passwords (Argon2id, architecture.md section 6).</summary>
public interface IPasswordHasher
{
    /// <summary>Returns a self-describing hash string that includes the salt and the parameters used.</summary>
    string Hash(string password);

    /// <summary>
    /// Checks a password against a stored hash. When <paramref name="passwordHash"/> is null (no such
    /// account) it still does the same amount of work before returning false, so response times do
    /// not reveal which emails have accounts.
    /// </summary>
    bool Verify(string password, string? passwordHash);
}
