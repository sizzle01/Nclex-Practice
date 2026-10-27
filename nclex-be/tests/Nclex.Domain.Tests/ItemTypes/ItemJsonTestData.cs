using System.Text.Json;

namespace Nclex.Domain.Tests.ItemTypes;

/// <summary>Builds the JSON and rationales that item-type tests feed to modules.</summary>
internal static class ItemJsonTestData
{
    public static JsonElement Json(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    public static JsonElement Json(object value) => JsonSerializer.SerializeToElement(value, Nclex.Domain.ItemTypes.ItemJson.Options);

    /// <summary>Options "a", "b", ... with placeholder text.</summary>
    public static object Options(int count) =>
        new { options = Enumerable.Range(0, count).Select(i => new { id = OptionId(i), text = $"Option {OptionId(i)}" }) };

    /// <summary>A rationale with an explanation for options "a", "b", ... up to <paramref name="count"/>.</summary>
    public static Nclex.Domain.ItemTypes.Rationale RationaleFor(int count) =>
        new("Why the answer is right.", Enumerable.Range(0, count).ToDictionary(OptionId, i => $"Why {OptionId(i)}."));

    /// <summary>A SATA answer key with the given correct ids.</summary>
    public static JsonElement SataKey(params string[] correct) => Json(new { correct });

    /// <summary>A SATA response selecting the given ids.</summary>
    public static JsonElement SataResponse(params string[] selected) => Json(new { selected });

    public static string OptionId(int index) => ((char)('a' + index)).ToString();
}
