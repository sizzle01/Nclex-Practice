using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nclex.Infrastructure.Persistence;

/// <summary>
/// How the EF Core tools (<c>dotnet ef</c> and the migration bundle) create the context, instead of
/// starting the whole API host: the bundle that deploys run (architecture.md section 8) then needs
/// only this assembly, not the API's logging, auth and secrets.
/// </summary>
/// <remarks>
/// The bundle's <c>--connection</c> argument replaces the connection string below. For
/// <c>dotnet ef</c> on a developer machine it comes from <c>ConnectionStrings__Postgres</c>, falling
/// back to the docker compose database in the README.
/// </remarks>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string LocalComposeDatabase = "Host=localhost;Port=5432;Database=nclex;Username=nclex;Password=nclex";

    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres") ?? LocalComposeDatabase;
        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UsePostgres(connectionString);
        return new AppDbContext(options.Options);
    }
}
