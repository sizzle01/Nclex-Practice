using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nclex.Api.Auth;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Test-only endpoint behind the VerifiedEmail policy, until F08 uses it for attempts.</summary>
[ApiController]
public sealed class VerifiedOnlyController : ControllerBase
{
    public const string Path = "/__test/verified-only";

    [HttpGet(Path)]
    [Authorize(Policy = Policies.VerifiedEmail)]
    public IActionResult Get() => Ok();
}
