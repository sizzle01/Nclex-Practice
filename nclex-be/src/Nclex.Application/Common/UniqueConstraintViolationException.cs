namespace Nclex.Application.Common;

/// <summary>
/// A save broke a unique constraint, for example two sign-ups racing for the same email.
/// Thrown by the database context so use cases can react without knowing about Npgsql.
/// </summary>
public sealed class UniqueConstraintViolationException(string? constraintName, Exception innerException)
    : Exception($"Unique constraint '{constraintName}' was violated.", innerException)
{
    /// <summary>Name of the violated constraint or unique index, e.g. "ix_users_email".</summary>
    public string? ConstraintName { get; } = constraintName;
}
