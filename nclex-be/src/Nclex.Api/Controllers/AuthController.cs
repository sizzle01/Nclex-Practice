using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Auth;
using Nclex.Application.Auth.Login;
using Nclex.Application.Auth.Logout;
using Nclex.Application.Auth.LogoutAll;
using Nclex.Application.Auth.Refresh;
using Nclex.Application.Auth.Signup;
using Nclex.Application.Common;

namespace Nclex.Api.Controllers;

/// <summary>
/// Sign-up, login and sessions (api.md section 2). The access token travels in response bodies;
/// the refresh token only in the HttpOnly cookie (see <see cref="RefreshTokenCookie"/>).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
public sealed class AuthController(TimeProvider clock) : ControllerBase
{
    /// <summary>Create an account and sign in.</summary>
    [HttpPost("signup")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Signup(
        SignupRequest request,
        [FromServices] SignupHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, HttpContext.GetClientInfo(), cancellationToken);
        RefreshTokenCookie.Write(Response, result.Session.RefreshToken, clock.GetUtcNow());
        return StatusCode(StatusCodes.Status201Created, AuthResponse.From(result));
    }

    /// <summary>Sign in with email and password.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        [FromServices] LoginHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, HttpContext.GetClientInfo(), cancellationToken);
        RefreshTokenCookie.Write(Response, result.Session.RefreshToken, clock.GetUtcNow());
        return Ok(AuthResponse.From(result));
    }

    /// <summary>Rotate the refresh-token cookie and get a new access token.</summary>
    /// <param name="requestedWith">Must be "fetch". Browsers do not add custom headers to cross-site form posts, so this blocks CSRF.</param>
    /// <param name="handler">Use case.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType<AccessTokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<ActionResult<AccessTokenResponse>> Refresh(
        [FromHeader(Name = RefreshTokenCookie.RequestedWithHeader)] string? requestedWith,
        [FromServices] RefreshHandler handler,
        CancellationToken cancellationToken)
    {
        if (requestedWith != RefreshTokenCookie.RequestedWithValue)
        {
            throw new AppException(CommonErrors.Unauthenticated);
        }

        try
        {
            var session = await handler.HandleAsync(RefreshTokenCookie.Read(Request), HttpContext.GetClientInfo(), cancellationToken);
            RefreshTokenCookie.Write(Response, session.RefreshToken, clock.GetUtcNow());
            return Ok(AccessTokenResponse.From(session));
        }
        catch (AppException)
        {
            // The cookie is dead (unknown, expired or revoked), so tell the browser to drop it.
            // Registered with OnStarting because the exception handler clears headers set before it runs.
            Response.OnStarting(() =>
            {
                RefreshTokenCookie.Delete(Response);
                return Task.CompletedTask;
            });
            throw;
        }
    }

    /// <summary>Sign out on this device. Always succeeds.</summary>
    [HttpPost("logout")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout([FromServices] LogoutHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(RefreshTokenCookie.Read(Request), cancellationToken);
        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }

    /// <summary>Sign out on every device.</summary>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public async Task<IActionResult> LogoutAll([FromServices] LogoutAllHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(cancellationToken);
        RefreshTokenCookie.Delete(Response);
        return NoContent();
    }
}
