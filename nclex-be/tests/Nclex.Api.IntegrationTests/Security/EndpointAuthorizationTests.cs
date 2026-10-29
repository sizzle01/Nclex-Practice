using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.Auth;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Security;

/// <summary>
/// Guards over every endpoint the API maps (architecture.md section 6), so a new endpoint cannot
/// quietly skip the rules. The per-endpoint behaviour is tested next to each feature; these tests
/// only check that nothing is left out.
/// </summary>
public sealed class EndpointAuthorizationTests(ApiFactory factory)
{
    /// <summary>
    /// The endpoints that may be called without a bearer token, and why. Adding one here is a
    /// security decision: check it against api.md before you do.
    /// </summary>
    private static readonly HashSet<string> AnonymousEndpoints =
    [
        // Before sign-in, or by token in an email link (api.md section 2).
        "POST /api/v1/auth/signup",
        "POST /api/v1/auth/login",
        "POST /api/v1/auth/verify-email",
        "POST /api/v1/auth/password/forgot",
        "POST /api/v1/auth/password/reset",
        "POST /api/v1/auth/email/confirm-change",
        // Authenticated by the path-scoped refresh cookie instead (architecture.md section 6).
        "POST /api/v1/auth/refresh",
        "POST /api/v1/auth/logout",
        // Authenticated by the signed link's HMAC (api.md section 10).
        "GET /api/v1/media/{id:guid}/file",
        "GET /api/v1/media/{id:guid}/captions",
        "GET /api/v1/media/{id:guid}/transcript",
        // Authenticated by a single-use ticket, since EventSource cannot send headers (api.md section 6).
        "GET /api/v1/notifications/stream",
        // Probes and the contract the frontend client is generated from (api.md section 9, ADR-8).
        "GET /healthz",
        "GET /readyz",
        "GET /api/v1/openapi.json",
    ];

    [Fact]
    public void Only_the_listed_endpoints_allow_anonymous_callers()
    {
        var anonymous = ApiEndpoints()
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            .Select(Describe)
            .ToHashSet();

        anonymous.Order().ShouldBe(AnonymousEndpoints.Order());
    }

    [Fact]
    public void Every_other_endpoint_requires_a_signed_in_user()
    {
        var unguarded = ApiEndpoints()
            .Where(e => e.Metadata.GetMetadata<IAllowAnonymous>() is null)
            .Where(e => !e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any())
            .Select(Describe)
            .ToList();

        unguarded.ShouldBeEmpty();
    }

    [Fact]
    public void Every_admin_endpoint_requires_a_staff_role()
    {
        // api.md section 8: admin routes are for authors, reviewers and admins only.
        string[] staffPolicies = [Policies.Author, Policies.Reviewer, Policies.Admin, Policies.ContentModerator];
        var adminWithoutRole = ApiEndpoints()
            .Where(e => e.RoutePattern.RawText?.TrimStart('/').StartsWith("api/v1/admin", StringComparison.Ordinal) == true)
            .Where(e => !e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any(a => staffPolicies.Contains(a.Policy) || a.Roles is not null))
            .Select(Describe)
            .ToList();

        adminWithoutRole.ShouldBeEmpty();
    }

    /// <summary>
    /// The contract test of architecture.md section 6: in what candidates can call, the answer key
    /// and rationale only appear in ItemReveal, which the reveal service fills once the rules allow.
    /// Admin schemas (the question editor) are left out: authors need the key.
    /// </summary>
    [Fact]
    public async Task Only_the_reveal_schema_carries_the_answer_key_or_rationale_for_candidates()
    {
        using var document = await OpenApiDocumentAsync();
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        var candidateSchemas = SchemasReachableFrom(CandidateOperations(document.RootElement), schemas);
        var withSecrets = candidateSchemas
            .Where(name => schemas.GetProperty(name).TryGetProperty("properties", out var properties)
                && (properties.TryGetProperty("answerKey", out _) || properties.TryGetProperty("rationale", out _)))
            .ToList();

        withSecrets.ShouldBe(["ItemReveal"]);
    }

    private IEnumerable<RouteEndpoint> ApiEndpoints() =>
        factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            // Test-only controllers from this assembly are not part of the API.
            .Where(e => e.RoutePattern.RawText?.Contains("__test", StringComparison.Ordinal) != true);

    private static string Describe(RouteEndpoint endpoint)
    {
        var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.Single() ?? "ANY";
        return $"{method} /{endpoint.RoutePattern.RawText?.TrimStart('/')}";
    }

    private async Task<JsonDocument> OpenApiDocumentAsync()
    {
        var client = factory.CreateClient();
        var json = await client.GetStringAsync("/api/v1/openapi.json", TestContext.Current.CancellationToken);
        return JsonDocument.Parse(json);
    }

    private static IEnumerable<JsonElement> CandidateOperations(JsonElement document) =>
        document.GetProperty("paths").EnumerateObject()
            .Where(path => !path.Name.StartsWith("/api/v1/admin", StringComparison.Ordinal))
            .SelectMany(path => path.Value.EnumerateObject().Select(operation => operation.Value));

    /// <summary>Names of every component schema the given operations use, directly or nested.</summary>
    private static HashSet<string> SchemasReachableFrom(IEnumerable<JsonElement> roots, JsonElement schemas)
    {
        var found = new HashSet<string>();
        var pending = new Stack<JsonElement>(roots);
        while (pending.TryPop(out var element))
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        if (property.Name == "$ref" && property.Value.GetString() is { } reference)
                        {
                            var name = reference[(reference.LastIndexOf('/') + 1)..];
                            if (found.Add(name))
                            {
                                pending.Push(schemas.GetProperty(name));
                            }
                        }
                        else
                        {
                            pending.Push(property.Value);
                        }
                    }

                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        pending.Push(item);
                    }

                    break;
            }
        }

        return found;
    }
}
