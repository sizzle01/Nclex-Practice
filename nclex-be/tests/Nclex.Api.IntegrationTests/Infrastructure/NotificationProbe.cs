using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Domain.Notifications;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Reads notification rows straight from the database, including email-only ones the API hides.</summary>
public sealed class NotificationProbe(ApiFactory factory)
{
    public async Task<List<Notification>> ForAsync(Guid userId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Notifications.AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>Runs SQL against the shared database, e.g. to age a draft.</summary>
    public async Task ExecuteAsync(FormattableString sql)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database
            .ExecuteSqlAsync(sql, TestContext.Current.CancellationToken);
    }
}
