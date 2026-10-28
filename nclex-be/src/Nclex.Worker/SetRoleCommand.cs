using Microsoft.EntityFrameworkCore;
using Nclex.Application.Abstractions;
using Nclex.Application.Auth;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Infrastructure;

namespace Nclex.Worker;

/// <summary>
/// <c>dotnet run --project src/Nclex.Worker -- set-role &lt;email&gt; &lt;role&gt;</c>: gives an
/// existing account a role. Only an admin can change roles in the console, so this is how the first
/// admin (and test accounts on a developer's machine) get theirs. It needs database access, which
/// already means operator access.
/// </summary>
internal static class SetRoleCommand
{
    public const string Name = "set-role";

    private const int Success = 0;
    private const int NotFound = 1;
    private const int BadUsage = 2;

    public static async Task<int> RunAsync(string[] args)
    {
        var roles = Enum.GetValues<UserRole>().ToDictionary(r => r.ToClaimValue(), StringComparer.Ordinal);
        if (args.Length != 2 || !roles.TryGetValue(args[1], out var role))
        {
            await Console.Error.WriteLineAsync($"Usage: {Name} <email> <{string.Join('|', roles.Keys)}>");
            return BadUsage;
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Services.AddInfrastructure();
        builder.Services.AddSingleton(TimeProvider.System);
        using var host = builder.Build();

        await using var scope = host.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditLog>();
        var clock = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        // Email is citext, so this ignores case like login does.
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == args[0] && u.DeletedAt == null);
        if (user is null)
        {
            await Console.Error.WriteLineAsync($"No account with email {args[0]}. Sign up first, then run this again.");
            return NotFound;
        }

        var previous = user.Role;
        user.ChangeRole(role, clock.GetUtcNow());
        audit.RecordAdminEvent(AuditActions.UserUpdated, actorId: null, AuditActions.UserEntity, user.Id, new { fromRole = previous, toRole = role, via = Name });
        await db.SaveChangesAsync();
        Console.WriteLine($"{user.Email} is now {args[1]} (was {previous.ToClaimValue()}). Sign in again to use the new role.");
        return Success;
    }
}
