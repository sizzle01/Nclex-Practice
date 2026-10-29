using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Banks;

namespace Nclex.Api.Controllers;

/// <summary>Question banks and their membership (api.md section 8, admin only).</summary>
[ApiController]
[Route("api/v1/admin/question-banks")]
[Authorize(Policy = Policies.Admin)]
[Produces("application/json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
public sealed class AdminQuestionBanksController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AdminBankPage>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public Task<AdminBankPage> List(
        [FromQuery] ListAdminBanksRequest request,
        [FromServices] AdminBanksHandler handler,
        CancellationToken cancellationToken) =>
        handler.ListAsync(request, cancellationToken);

    [HttpGet("{id:guid}")]
    [ProducesResponseType<AdminBankView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminBankView> Get(
        Guid id,
        [FromServices] AdminBanksHandler handler,
        CancellationToken cancellationToken) =>
        handler.GetAsync(id, cancellationToken);

    [HttpPost]
    [ProducesResponseType<AdminBankView>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    public async Task<IActionResult> Create(
        CreateBankRequest request,
        [FromServices] AdminBanksHandler handler,
        CancellationToken cancellationToken)
    {
        var created = await handler.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType<AdminBankView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminBankView> Update(
        Guid id,
        UpdateBankRequest request,
        [FromServices] AdminBanksHandler handler,
        CancellationToken cancellationToken) =>
        handler.UpdateAsync(id, request, cancellationToken);
}
