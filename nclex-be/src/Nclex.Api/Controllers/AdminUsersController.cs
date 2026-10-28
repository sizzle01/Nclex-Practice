using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Users;

namespace Nclex.Api.Controllers;

/// <summary>User management: role, plan tier and unlock (api.md section 8, admin only).</summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminUsersController : ControllerBase
{
    /// <summary>Search accounts by email or display name.</summary>
    [HttpGet]
    [ProducesResponseType<AdminUserPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<AdminUserPage> List(
        [FromQuery] ListAdminUsersRequest request,
        [FromServices] AdminUsersHandler handler,
        CancellationToken cancellationToken) =>
        handler.ListAsync(request, cancellationToken);

    /// <summary>Change role or plan tier, or unlock. A role change signs the user out everywhere.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType<AdminUserView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminUserView> Update(
        Guid id,
        UpdateAdminUserRequest request,
        [FromServices] AdminUsersHandler handler,
        CancellationToken cancellationToken) =>
        handler.UpdateAsync(id, request, cancellationToken);
}
