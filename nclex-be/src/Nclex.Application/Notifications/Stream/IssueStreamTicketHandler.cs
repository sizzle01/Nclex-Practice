using Nclex.Application.Abstractions;
using Nclex.Application.Common;

namespace Nclex.Application.Notifications.Stream;

/// <summary>Body of POST /notifications/stream-ticket (api.md section 6).</summary>
public sealed record StreamTicketResponse(string Ticket, int ExpiresInSec);

/// <summary>POST /notifications/stream-ticket: a single-use ticket to open the SSE stream with.</summary>
public sealed class IssueStreamTicketHandler(IStreamTicketStore tickets, ICurrentUser currentUser)
{
    /// <summary>Long enough to open the stream right after asking, short enough that a leaked URL is useless.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(30);

    public async Task<StreamTicketResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var ticket = await tickets.IssueAsync(currentUser.UserId, Lifetime, cancellationToken)
            ?? throw new AppException(NotificationErrors.StreamUnavailable);
        return new StreamTicketResponse(ticket, (int)Lifetime.TotalSeconds);
    }
}
