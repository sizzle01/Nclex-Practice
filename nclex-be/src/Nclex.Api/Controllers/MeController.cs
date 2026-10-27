using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Users;
using Nclex.Application.Users.ChangeEmail;
using Nclex.Application.Users.ChangePassword;
using Nclex.Application.Users.DeleteAccount;
using Nclex.Application.Users.GetMe;
using Nclex.Application.Users.UpdateMe;

namespace Nclex.Api.Controllers;

/// <summary>
/// The signed-in user's own account (api.md section 2). Changing the password or email and
/// deleting the account ask for the current password again; a wrong one is 400 VALIDATION_FAILED
/// on <c>currentPassword</c>, and too many wrong ones lock the account (423).
/// </summary>
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

    /// <summary>Change my password. This device stays signed in; all others are signed out.</summary>
    [HttpPost("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        [FromServices] ChangePasswordHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, HttpContext.GetClientInfo(), cancellationToken);
        return NoContent();
    }

    /// <summary>Ask to change my email. A confirmation link goes to the new address; nothing changes until it is used.</summary>
    [HttpPost("email")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public async Task<IActionResult> ChangeEmail(
        ChangeEmailRequest request,
        [FromServices] ChangeEmailHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, HttpContext.GetClientInfo(), cancellationToken);
        return Accepted();
    }

    /// <summary>Delete my account. Personal data is removed and every session ends.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked, "application/problem+json")]
    public async Task<IActionResult> Delete(
        DeleteAccountRequest request,
        [FromServices] DeleteAccountHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request, HttpContext.GetClientInfo(), cancellationToken);
        // The browser's refresh cookie points at a session that no longer exists.
        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }
}
