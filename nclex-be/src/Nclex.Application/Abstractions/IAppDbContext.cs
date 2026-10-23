namespace Nclex.Application.Abstractions;

/// <summary>
/// The application's view of the database. Use cases depend on this interface rather than on
/// EF Core's concrete context, which lives in Nclex.Infrastructure (architecture.md section 4).
/// Entity sets are added here as features introduce tables.
/// </summary>
public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
