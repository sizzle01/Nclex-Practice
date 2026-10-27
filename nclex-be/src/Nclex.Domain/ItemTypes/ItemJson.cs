using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nclex.Domain.ItemTypes;

/// <summary>
/// JSON settings for item payloads: camelCase like the API (interfaces.md section 3), and strict,
/// so a payload missing a required field fails to read instead of becoming a half-empty record.
/// </summary>
public static class ItemJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };
}
