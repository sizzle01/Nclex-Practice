using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Test-only endpoint that fails, to check how unhandled exceptions reach the client.</summary>
[ApiController]
[AllowAnonymous]
public sealed class ThrowingController : ControllerBase
{
    public const string Path = "/__test/throw";
    public const string SecretMessage = "connection string with password=hunter2";

    [HttpGet(Path)]
    public IActionResult Throw() => throw new InvalidOperationException(SecretMessage);
}
