using System.Text.Json;
using System.Text.Json.Serialization;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Grading;

/// <summary>
/// How <c>attempt_items.grade_detail</c> is stored: the same camelCase shape as the API's
/// <c>reveal.grade.detail</c>, without empty fields (api.md, GET /items).
/// </summary>
public static class GradeDetailJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static JsonDocument Write(IReadOnlyList<GradeDetail> detail) =>
        JsonSerializer.SerializeToDocument(detail, Options);

    /// <summary>Reads stored detail back; <c>selected</c> and <c>expected</c> come back as JSON values.</summary>
    public static IReadOnlyList<GradeDetail> Read(JsonDocument json) =>
        json.Deserialize<List<GradeDetail>>(Options) ?? [];
}
