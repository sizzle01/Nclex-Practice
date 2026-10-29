using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;
using Nclex.Application.Admin.Media;
using Nclex.Domain.Content;

namespace Nclex.Api.Controllers;

/// <summary>
/// Uploading question media (api.md section 8). POST upload-url returns a signed link; the browser
/// PUTs the file there, as it would to a presigned S3 URL, then PATCHes alt text or captions.
/// </summary>
[ApiController]
[Route("api/v1/admin/media")]
[Authorize(Policy = Policies.ContentAuthor)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
public sealed class AdminMediaController : ControllerBase
{
    /// <summary>Record a new asset and get a link to upload its file to.</summary>
    [HttpPost("upload-url")]
    [Produces("application/json")]
    [ProducesResponseType<UploadUrlResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public Task<UploadUrlResponse> CreateUploadUrl(
        CreateUploadUrlRequest request,
        [FromServices] CreateUploadUrlHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(request, cancellationToken);

    /// <summary>
    /// The upload link: PUT the file's bytes with its Content-Type. No token: the signature is the
    /// permission, so the link works like a presigned URL.
    /// </summary>
    [HttpPut("{id:guid}/upload")]
    [AllowAnonymous]
    // A little over the largest allowed file, so a too-large body is refused by the handler's own check.
    [RequestSizeLimit(MediaUploadRules.MaxBytes + 1024)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public async Task<IActionResult> Upload(
        Guid id,
        [FromQuery] long exp,
        [FromQuery] string? sig,
        [FromServices] UploadMediaFileHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(id, exp, sig, Request.ContentType, Request.Body, cancellationToken);
        return NoContent();
    }

    /// <summary>Set alt text (images), or captions and a transcript (audio and video).</summary>
    [HttpPatch("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType<AdminMediaView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
    public Task<AdminMediaView> Update(
        Guid id,
        UpdateMediaRequest request,
        [FromServices] UpdateMediaHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, request, cancellationToken);
}
