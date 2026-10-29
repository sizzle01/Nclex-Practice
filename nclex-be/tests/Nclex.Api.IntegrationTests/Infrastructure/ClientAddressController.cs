using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Test-only endpoint that reports the client address and scheme the API settled on.</summary>
[ApiController]
[AllowAnonymous]
public sealed class ClientAddressController : ControllerBase
{
    public const string Path = "/__test/client-address";

    [HttpGet(Path)]
    public ClientAddress Get() => new(HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Scheme);

    public sealed record ClientAddress(string? Ip, string Scheme);
}
