using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Nclex.Api.OpenApi;

/// <summary>
/// Names query parameters in camelCase in the OpenAPI document, as api.md writes them
/// (<c>?track=RN&amp;kind=prebuilt</c>). Query strings bound to a request record are otherwise
/// documented with the C# property names ("Track"), and the generated frontend client would use
/// those. Model binding ignores case, so both spellings reach the API the same way.
/// </summary>
internal sealed class CamelCaseQueryParameters : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is { Length: > 0 } name && char.IsUpper(name[0]))
            {
                parameter.Name = char.ToLowerInvariant(name[0]) + name[1..];
            }
        }

        return Task.CompletedTask;
    }
}
