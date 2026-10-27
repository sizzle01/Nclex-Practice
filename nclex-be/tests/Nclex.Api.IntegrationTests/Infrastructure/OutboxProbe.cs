using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Domain.Email;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Reads the email outbox the way a mailbox would be read: the API tests never run the worker, so
/// the raw link token is still in the queued row's payload.
/// </summary>
public sealed class OutboxProbe(ApiFactory factory)
{
    /// <summary>Emails queued for <paramref name="email"/>, oldest first.</summary>
    public async Task<List<EmailOutboxMessage>> ForAsync(string email)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.EmailOutbox
            .Where(m => m.ToEmail == email)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    /// <summary>The link token in the newest <paramref name="template"/> email to <paramref name="email"/>.</summary>
    public async Task<string> LatestTokenAsync(string email, string template)
    {
        var message = (await ForAsync(email)).Last(m => m.Template == template);
        return TokenIn(message);
    }

    public static string TokenIn(EmailOutboxMessage message)
    {
        using var payload = JsonDocument.Parse(message.Payload);
        return payload.RootElement.GetProperty("token").GetString()!;
    }
}
