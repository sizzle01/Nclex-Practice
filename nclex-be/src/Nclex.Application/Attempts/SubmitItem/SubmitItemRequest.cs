using System.Text.Json;

namespace Nclex.Application.Attempts.SubmitItem;

/// <summary>
/// Body of POST /attempts/{id}/items/{position}/submit (api.md section 4). Both fields are optional:
/// send them to save a last answer before the item locks, as the save queue's final flush.
/// </summary>
/// <param name="Response">The final answer; null clears it.</param>
/// <param name="ClientSeq">Required with a final answer; an older one than the stored answer is ignored.</param>
public sealed record SubmitItemRequest(JsonElement? Response = null, int? ClientSeq = null);
