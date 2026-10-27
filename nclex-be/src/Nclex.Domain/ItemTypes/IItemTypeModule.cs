using System.Text.Json;

namespace Nclex.Domain.ItemTypes;

/// <summary>
/// The plug-in boundary for one item type (interfaces.md section 3). Works on raw JSON so the
/// registry and application services can handle every type the same way. Implementations derive
/// from <see cref="ItemTypeModule{TContent, TKey, TResponse}"/> rather than implementing this directly.
/// </summary>
public interface IItemTypeModule
{
    ItemType Type { get; }

    Phase Phase { get; }

    /// <summary>
    /// Authoring-time checks: the payloads have the right shape, the key refers to real ids, and
    /// <c>rationale.perOption</c> covers every selectable id. An empty list means the item is valid.
    /// </summary>
    IReadOnlyList<ValidationIssue> ValidateAuthoring(JsonElement content, JsonElement key, Rationale rationale);

    /// <summary>Autosave checks on a candidate's response against the item's content.</summary>
    IReadOnlyList<ValidationIssue> ValidateResponse(JsonElement content, JsonElement response);

    /// <summary>The most points the item can earn; stored as <c>question_versions.max_points</c>.</summary>
    decimal MaxPoints(JsonElement content, JsonElement key);

    /// <summary>Scores a response. Pure and deterministic. A null response (blank or timed out) scores 0.</summary>
    GradeResult Grade(JsonElement content, JsonElement key, JsonElement? response);

    /// <summary>
    /// The content as one attempt shows it, e.g. with options in a seeded order. Ids are kept, so
    /// grading is unaffected.
    /// </summary>
    JsonElement Present(JsonElement content, string seed);
}
