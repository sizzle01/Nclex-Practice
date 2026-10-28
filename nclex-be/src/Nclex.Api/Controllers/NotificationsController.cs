using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Abstractions;
using Nclex.Application.Notifications.Feed;
using Nclex.Application.Notifications.ReadState;
using Nclex.Application.Notifications.Stream;

namespace Nclex.Api.Controllers;

/// <summary>
/// The signed-in user's notifications (api.md section 6). The live stream itself is not a
/// controller action: see <see cref="Sse.NotificationStreamEndpoint"/>.
/// </summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class NotificationsController : ControllerBase
{
    /// <summary>My in-app notifications, newest first.</summary>
    [HttpGet]
    [ProducesResponseType<NotificationPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<NotificationPage> List(
        [FromQuery] ListNotificationsRequest request,
        [FromServices] ListNotificationsHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>How many of my notifications are unread (the bell's badge).</summary>
    [HttpGet("unread-count")]
    [ProducesResponseType<UnreadCountResponse>(StatusCodes.Status200OK)]
    public Task<UnreadCountResponse> UnreadCount(
        [FromServices] UnreadCountHandler handler,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(currentUser.UserId, cancellationToken);

    /// <summary>Mark one notification read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> MarkRead(
        Guid id,
        [FromServices] MarkReadHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>Mark all my notifications read.</summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllRead([FromServices] MarkAllReadHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// A 30-second, single-use ticket for GET /notifications/stream, because EventSource cannot send
    /// the Authorization header. 503 STREAM_UNAVAILABLE when live updates are down; poll instead.
    /// </summary>
    [HttpPost("stream-ticket")]
    [ProducesResponseType<StreamTicketResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable, "application/problem+json")]
    public Task<StreamTicketResponse> StreamTicket(
        [FromServices] IssueStreamTicketHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(cancellationToken);
}
