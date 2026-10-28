namespace Nclex.Application.Abstractions;

/// <summary>
/// Short-lived, single-use tickets for the notification stream. EventSource cannot send an
/// Authorization header (api.md section 6), so the client trades its access token for a ticket
/// and puts the ticket in the stream's URL instead.
/// </summary>
public interface IStreamTicketStore
{
    /// <summary>Creates a ticket for <paramref name="userId"/>; null when the store is unavailable.</summary>
    Task<string?> IssueAsync(Guid userId, TimeSpan lifetime, CancellationToken cancellationToken);

    /// <summary>The ticket's user, if it is valid and unused. A ticket works once.</summary>
    Task<Guid?> RedeemAsync(string ticket, CancellationToken cancellationToken);
}
