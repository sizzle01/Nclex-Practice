using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Notifications.Preferences;

namespace Nclex.Api.Controllers;

/// <summary>
/// Which notifications I get in-app and by email, per category (api.md section 6). Security is
/// locked on; a request that turns it off is 400 VALIDATION_FAILED.
/// </summary>
[ApiController]
[Route("api/v1/me/notification-preferences")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class NotificationPreferencesController : ControllerBase
{
    /// <summary>My preferences, one entry per category.</summary>
    [HttpGet]
    [ProducesResponseType<NotificationPreferencesResponse>(StatusCodes.Status200OK)]
    public Task<NotificationPreferencesResponse> Get(
        [FromServices] GetPreferencesHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(cancellationToken);

    /// <summary>Replace my preferences; every category must be sent once.</summary>
    [HttpPut]
    [ProducesResponseType<NotificationPreferencesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<NotificationPreferencesResponse> Update(
        UpdatePreferencesRequest request,
        [FromServices] UpdatePreferencesHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
