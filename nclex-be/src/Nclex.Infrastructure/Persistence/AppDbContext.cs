using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Persistence;

/// <summary>The EF Core context for the PostgreSQL system of record.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // database.md section 2.1: pgcrypto provides gen_random_uuid() for primary keys,
        // citext gives case-insensitive email columns.
        modelBuilder.HasPostgresExtension("pgcrypto");
        modelBuilder.HasPostgresExtension("citext");
    }
}
