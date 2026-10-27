using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Domain.Audit;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Reads <c>audit_events</c> rows about one user.</summary>
public sealed class AuditProbe(ApiFactory factory)
{
    /// <summary>Events whose actor is <paramref name="userId"/>, oldest first.</summary>
    public async Task<List<AuditEvent>> ForUserAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AuditEvents
            .Where(a => a.ActorId == userId)
            .OrderBy(a => a.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Just the action names, oldest first.</summary>
    public async Task<List<string>> ActionsForAsync(Guid userId) =>
        [.. (await ForUserAsync(userId)).Select(a => a.Action)];
}
