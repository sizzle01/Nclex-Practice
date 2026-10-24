using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Users;
using Nclex.Application.Users.GetMe;
using Nclex.Application.Users.UpdateMe;

namespace Nclex.Api.Controllers;

/// <summary>The signed-in user's own profile (api.md section 2).</summary>
[ApiController]
[Route("api/v1/me")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
public sealed class MeController : ControllerBase
{
    /// <summary>Get my profile.</summary>
    [HttpGet]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    public Task<UserProfile> Get([FromServices] GetMeHandler handler, CancellationToken cancellationToken) =>
        handler.HandleAsync(cancellationToken);

    /// <summary>Update my display name, track, time zone or target exam date.</summary>
    [HttpPatch]
    [ProducesResponseType<UserProfile>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<UserProfile> Update(
        UpdateMeRequest request,
        [FromServices] UpdateMeHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);
}
