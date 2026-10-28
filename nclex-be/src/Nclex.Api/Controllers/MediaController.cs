using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Application.Abstractions;
using Nclex.Application.Media.GetMedia;
using Nclex.Application.Media.OpenMediaFile;

namespace Nclex.Api.Controllers;

/// <summary>
/// Question media (api.md section 10): signed links for a signed-in candidate, and the files
/// those links open. The file endpoints take no token, because &lt;img&gt; and &lt;audio&gt; can't
/// send one; the link's signature is the permission.
/// </summary>
[ApiController]
[Route("api/v1/media")]
[Authorize]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
public sealed class MediaController : ControllerBase
{
    /// <summary>The asset's kind, alt text and size, with links to its file and captions that work for a few minutes.</summary>
    [HttpGet("{id:guid}")]
    [Produces("application/json")]
    [ProducesResponseType<MediaView>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public Task<MediaView> Get(
        Guid id,
        [FromServices] GetMediaHandler handler,
        CancellationToken cancellationToken) =>
        handler.HandleAsync(id, cancellationToken);

    /// <summary>The image, audio or video file. Supports range requests, so clips can seek.</summary>
    [HttpGet("{id:guid}/file")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public Task<IActionResult> GetFile(
        Guid id,
        [FromQuery] long exp,
        [FromQuery] string? sig,
        [FromServices] OpenMediaFileHandler handler,
        CancellationToken cancellationToken) =>
        StreamAsync(handler.HandleAsync(id, MediaPart.File, exp, sig, cancellationToken));

    /// <summary>The WebVTT captions of an audio or video clip.</summary>
    [HttpGet("{id:guid}/captions")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, "application/problem+json")]
    public Task<IActionResult> GetCaptions(
        Guid id,
        [FromQuery] long exp,
        [FromQuery] string? sig,
        [FromServices] OpenMediaFileHandler handler,
        CancellationToken cancellationToken) =>
        StreamAsync(handler.HandleAsync(id, MediaPart.Captions, exp, sig, cancellationToken));

    private async Task<IActionResult> StreamAsync(Task<MediaContent> opening)
    {
        var content = await opening;
        // Private: the link is personal and short-lived, so shared caches must not keep it.
        Response.Headers.CacheControl = "private, max-age=600";
        return File(content.Stream, content.ContentType, enableRangeProcessing: true);
    }
}
