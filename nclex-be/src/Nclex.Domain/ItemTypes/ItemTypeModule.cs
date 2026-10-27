using System.Text.Json;

namespace Nclex.Domain.ItemTypes;

/// <summary>
/// Base class for item-type modules: reads the JSON payloads into the type's own records once, then
/// calls the strongly typed overrides (interfaces.md section 3).
/// </summary>
/// <typeparam name="TContent">What candidates see, e.g. the options.</typeparam>
/// <typeparam name="TKey">The answer key. Never sent to candidates before the reveal.</typeparam>
/// <typeparam name="TResponse">The candidate's answer.</typeparam>
public abstract class ItemTypeModule<TContent, TKey, TResponse> : IItemTypeModule
    where TContent : class
    where TKey : class
    where TResponse : class
{
    public abstract ItemType Type { get; }

    public abstract Phase Phase { get; }

    public abstract decimal MaxPoints(TContent content, TKey key);

    public abstract GradeResult Grade(TContent content, TKey key, TResponse? response);

    public virtual IReadOnlyList<ValidationIssue> ValidateAuthoring(TContent content, TKey key, Rationale rationale) => [];

    public virtual IReadOnlyList<ValidationIssue> ValidateResponse(TContent content, TResponse response) => [];

    public virtual TContent Present(TContent content, string seed) => content;

    IReadOnlyList<ValidationIssue> IItemTypeModule.ValidateAuthoring(JsonElement content, JsonElement key, Rationale rationale)
    {
        var contentRead = TryRead<TContent>(content, "content", out var typedContent);
        var keyRead = TryRead<TKey>(key, "key", out var typedKey);
        if (contentRead is not null || keyRead is not null)
        {
            return new[] { contentRead, keyRead }.OfType<ValidationIssue>().ToList();
        }

        return ValidateAuthoring(typedContent!, typedKey!, rationale);
    }

    IReadOnlyList<ValidationIssue> IItemTypeModule.ValidateResponse(JsonElement content, JsonElement response)
    {
        var problem = TryRead<TResponse>(response, "response", out var typedResponse);
        return problem is not null ? [problem] : ValidateResponse(Read<TContent>(content), typedResponse!);
    }

    decimal IItemTypeModule.MaxPoints(JsonElement content, JsonElement key) =>
        MaxPoints(Read<TContent>(content), Read<TKey>(key));

    GradeResult IItemTypeModule.Grade(JsonElement content, JsonElement key, JsonElement? response)
    {
        var typedResponse = response is { ValueKind: not (JsonValueKind.Null or JsonValueKind.Undefined) } value
            ? Read<TResponse>(value)
            : null;
        return Grade(Read<TContent>(content), Read<TKey>(key), typedResponse);
    }

    JsonElement IItemTypeModule.Present(JsonElement content, string seed) =>
        JsonSerializer.SerializeToElement(Present(Read<TContent>(content), seed), ItemJson.Options);

    // Content and keys are validated when authored, so a failure to read them here is a bug, not bad input.
    private static T Read<T>(JsonElement json)
        where T : class =>
        json.Deserialize<T>(ItemJson.Options)
        ?? throw new JsonException($"Expected a {typeof(T).Name} object but found null.");

    private static ValidationIssue? TryRead<T>(JsonElement json, string path, out T? value)
        where T : class
    {
        value = null;
        if (json.ValueKind != JsonValueKind.Object)
        {
            return new ValidationIssue(path, ValidationIssueCodes.InvalidShape, $"The {path} must be a JSON object.");
        }

        try
        {
            value = Read<T>(json);
            return null;
        }
        catch (JsonException ex)
        {
            return new ValidationIssue(path, ValidationIssueCodes.InvalidShape, $"The {path} does not have the expected fields: {ex.Message}");
        }
    }
}
